import time


class AgentMetrics:
    def __init__(self):
        self.start_inference_time = None
        self.total_invalid_commands = 0
        self.total_number_of_commands = 0
        self.total_inference_count = 0
        self.total_inference_time = 0.0

    def start_inference_timer(self):
        self.start_inference_time = time.time()

    def stop_inference_timer(self):
        run_inference_time = time.time() - self.start_inference_time
        self.total_inference_time += run_inference_time

    def print_metrics(self):
        print("Total_number_of_commands: ", self.total_number_of_commands)
        print("Total_inference_count: ", self.total_inference_count)
        print("Total_inference_time: ", self.total_inference_time)
        print("Invalid commands: ", self.total_invalid_commands)
