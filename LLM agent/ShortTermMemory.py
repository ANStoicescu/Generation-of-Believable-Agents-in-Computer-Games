

class ShortTermMemory:
    def __init__(self, max_len):
        self.recent_memory_stream = []
        self. max_memory_len = max_len

    def get_recent_memories(self):
        memories = ""
        for memory in self.recent_memory_stream:
            memories += f"{memory['type']}: {memory['description']} \n"

    def prune_recent_memory(self):
        self.recent_memory_stream = self.recent_memory_stream[-self.max_memory_len:]

    def add_observation(self, observation):
        self.recent_memory_stream.append({
            'type': 'Seen observation',
            'description': observation
        })

    def add_action(self, action):
        self.recent_memory_stream.append({
            'type': 'Sent action',
            'description': action
        })

    def add_reflection(self, reflection):
        self.recent_memory_stream.append({
            'type': 'Reflection',
            'description': reflection,
        })

