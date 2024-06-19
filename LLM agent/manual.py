import time

from flask import Flask, request, jsonify

app = Flask(__name__)


def askLLM(question):
    #time.sleep(2)
    response = input()
    return response


@app.route('/agent_server/act', methods=['POST'])
def act():
    args = request.args
    parsed_request = request.json
    observation = parsed_request["observation"]


    print(observation)
    response = input()

    return jsonify({'response': response})


@app.route('/agent_server/observation', methods=['POST'])
def observation():
    args = request.args
    parsed_request = request.json
    observation = parsed_request["observation"]
    print(observation)

    return jsonify({'response': "Done"})



if __name__ == '__main__':
    app.run(threaded=False)