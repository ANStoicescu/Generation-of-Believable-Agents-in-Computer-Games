import os
from datetime import datetime
from flask import Flask, request, jsonify
from LongTermMemory import LongTermMemory
from ShortTermMemory import ShortTermMemory
from AgentMetrics import AgentMetrics
import pickle
import openai

engine = "dlcru-gpt4"

class GenerativeAgent:
    def __init__(self, name, description):
        self.name = name
        self.description = description
        self.tasks = ["Find eggs and a pan to make scrambled eggs."]
        self.engine = "dlcru-gpt4"
        # self.engine = "dlcru-gpt-35-turbo"
        self.short_term_memory = ShortTermMemory(10)
        self.long_term_memory = LongTermMemory('cosine')
        self.response = None
        self.system_prompt = f"Your character description: Your name is {self.name}. {self.description}"
        self.agent_metrics = AgentMetrics()

    def take_action(self, observation):
        self.engine = "dlcru-gpt-35-turbo"
        if self.response is not None:
            observation += f"This is the environment's response for the command: {self.response}"

        extracted_information = self.extract_essential_information(observation)
        self.long_term_memory.add_memory(extracted_information)
        self.short_term_memory.add_reflection(extracted_information)
        plan = self.make_plan(observation, self.tasks[0])

        prompt_msg = [{"role": "system", "content": self.system_prompt}]
        prompt = f"{plan}\n{extracted_information}\n{observation}."
        prompt_msg.append({"role": "user", "content": prompt})

        system_prompt = ("Write only the command you want to take. Write only the items you want to use with that "
                         "command not any resulting items.")
        prompt_msg.append({"role": "system", "content": system_prompt})

        action = self.get_model_response(prompt_msg, 0.02)
        print("Action: ", action)
        self.short_term_memory.add_action(action)
        self.short_term_memory.add_observation(observation)
        self.response = action
        self.short_term_memory.prune_recent_memory()

        self.agent_metrics.total_number_of_commands += 1

        return action

    def make_plan(self, observation, task):
        prompt = (
            f"\nYou are in a simplified version of the world. Some steps might not be necessary to achieve your goal."
            f" Think only based on what you observed and what commands the environment says are available."
            f"Ask yourself what items are missing from your inventory to complete the task. Where can you find them based on the available objects in the environment."
            f"Think of a short plan without bullet points using the following information and memories knowing that your task is to {task}."
            f"Keep the plan short and use only actions possible in this environment."
            f"This is the information you have:\n")

        prompt_msg = [{"role": "system", "content": self.system_prompt + prompt}]

        prompt = f"Given these recent experiences in your memory: {self.short_term_memory.get_recent_memories()}"
        prompt += "And this is the information received from the environment: " + observation

        prompt_msg.append({"role": "user", "content": prompt})

        plan = self.get_model_response(prompt_msg, 0.2)
        print("Plan: ", plan)

        return plan

    def reflect(self, observation):
        prompt_msg = [{"role": "system", "content": self.system_prompt}]

        prompt = f"Given these recent experiences:\n{self.short_term_memory.get_recent_memories()}\nWhat is a high-level insights you can infer?"
        prompt_msg.append({"role": "user", "content": prompt})
        response = self.get_model_response(prompt_msg, 0.5)
        print("Reflection response", response)
        reflections = response.split('\n')
        for reflection in reflections:
            print("Reflection", reflection)
            self.short_term_memory.add_reflection(reflection)

    def talk(self, conversation):
        prompt = (
            "You are engaged in a conversation with another agent, stick to your character description while you answer. "
            "Keep the conversation human like with short replies.")

        prompt_msg = [{"role": "system", "content": self.system_prompt + prompt}]

        for msg in conversation:
            if msg[0] == self.name:
                prompt_msg.append({"role": "assistant", "content": msg[1]})
            else:
                prompt_msg.append({"role": "user", "content": msg[1]})

        reply = self.get_model_response(prompt_msg, 0.25)

        return reply

    def get_model_response(self, messages, temperature):
        self.agent_metrics.total_inference_count += 1

        if self.engine == "dlcru-gpt-35-turbo":
            openai.api_type = "azure"
            openai.api_base = ""
            openai.api_version = ""
            openai.api_key = ""

        if self.engine == "dlcru-gpt4":
            openai.api_type = "azure"
            openai.api_base = ""
            openai.api_version = ""
            openai.api_key = ""

        completion = openai.ChatCompletion.create(
            engine= self.engine,
            messages=messages,
            temperature=temperature)

        return completion.choices[0].message['content']

    def extract_essential_information(self, observation):
        prompt_msg = [
            {"role": "system", "content": "You are inside a sims like environment and you need to solve some tasks."}]

        prompt = (f"Given these recent experiences:\n{self.short_term_memory.get_recent_memories()}\n"
                  f"What is the most important information that you can extract from this text: {observation}")
        prompt_msg.append({"role": "user", "content": prompt})
        response = self.get_model_response(prompt_msg, 0.2)

        return response


app = Flask(__name__)
Agents = {}

agents_save_dir = "Agents Saves"


@app.route('/agent_server/act', methods=['POST'])
def act():
    args = request.args
    currentAgent = Agents[args["agentName"]]
    parsed_request = request.json
    observation = parsed_request["observation"]

    if "Error" in observation:
        currentAgent.agent_metrics.total_invalid_commands += 1

    currentAgent.agent_metrics.start_inference_timer()
    # if currentAgent.response:
    #     currentAgent.reflect(observation)

    print(observation)
    response = currentAgent.take_action(observation)

    currentAgent.agent_metrics.stop_inference_timer()

    return jsonify({'response': response})


@app.route('/agent_server/observation', methods=['POST'])
def observation():
    args = request.args
    currentAgent = Agents[args["agentName"]]
    parsed_request = request.json
    observation = parsed_request["observation"]

    currentAgent.agent_metrics.start_inference_timer()

    if currentAgent.response:
        currentAgent.reflect(observation)

    print(currentAgent.long_term_memory.query("Talk to an agent!", 4))

    currentAgent.agent_metrics.stop_inference_timer()

    currentAgent.agent_metrics.print_metrics()

    return jsonify({'response': "Done"})


@app.route('/agent_server/talk', methods=['POST'])
def talk():
    args = request.args
    currentAgent = Agents[args["agentName"]]
    parsed_request = request.json
    requestedAgent = Agents[parsed_request["observation"]]

    conversational_agents = [currentAgent, requestedAgent]

    conversation_list = []

    max_turns = 10
    turn_count = 0

    response = ""

    while turn_count < max_turns or "end the conversation" in response.lower():
        response = conversational_agents[turn_count % 2].talk(conversation_list)
        conversation_list.append((conversational_agents[turn_count % 2].name, response))

        turn_count += 1

    return jsonify({'response': "Done"})


@app.route('/agent_server/initialize', methods=['POST'])
def initialize():
    args = request.args
    agentName = args["agentName"]
    parsed_request = request.json
    description = parsed_request["observation"]
    Agents[agentName] = GenerativeAgent(agentName, description)

    print(f"Created agent {agentName}")

    return jsonify({'result': 'Done'})


@app.route('/agent_server/save', methods=['POST'])
def save():
    args = request.args
    agentName = args["agentName"]
    parsed_request = request.json
    saveID = parsed_request["observation"]

    # Ensure the directory exists
    save_path = f'{agents_save_dir}/{saveID}'
    os.makedirs(save_path, exist_ok=True)

    pickle.dump(Agents[agentName], open(f'{save_path}/agent_{agentName}.pkl', 'wb'))

    print(f"Saved agent {agentName} from SaveID {saveID}")

    return jsonify({'result': 'Done'})


@app.route('/agent_server/load', methods=['POST'])
def load():
    args = request.args
    agentName = args["agentName"]
    parsed_request = request.json
    saveID = parsed_request["observation"]
    agent = pickle.load(open(f'{agents_save_dir}/{saveID}/agent_{agentName}.pkl', 'rb'))

    Agents[agentName] = agent

    print(f"Loaded agent {agentName} from SaveID {saveID}")

    return jsonify({'result': 'Done'})


if __name__ == '__main__':
    app.run(threaded=False)
