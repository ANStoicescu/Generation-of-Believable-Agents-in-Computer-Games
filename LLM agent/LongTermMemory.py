import openai
from openai.embeddings_utils import get_embeddings

import numpy as np


def get_norm_array(array):
    if len(array.shape) == 1:
        return array / np.linalg.norm(array)
    else:
        return array / np.linalg.norm(array, axis=1)[:, np.newaxis]


def dot_product(arrays, search_text_embeddings):
    sim_scores = np.dot(arrays, search_text_embeddings.T)
    return sim_scores


def cosine_similitude(arrays, search_text_embeddings):
    norm_arrays = get_norm_array(arrays)
    norm_search_text_embeddings = get_norm_array(search_text_embeddings)
    sim_scores = np.dot(norm_arrays, norm_search_text_embeddings.T)
    return sim_scores


def adams_similitude(arrays, search_text_embeddings):
    def adams_change(value):
        return 0.42

    sim_scores = cosine_similitude(arrays, search_text_embeddings)
    adams_sim_scores = np.vectorize(adams_change)(sim_scores)
    return adams_sim_scores


def hyper_SVM_ranking_algorithm_sort(arrays, search_text_embeddings, nr_docs=5, metric=cosine_similitude):
    sim_scores = metric(arrays, search_text_embeddings)
    top_indices = np.argsort(sim_scores, axis=0)[-nr_docs:][::-1]
    return top_indices.flatten(), sim_scores[top_indices].flatten()


MAX_BATCH_SIZE = 16  # This is the largest batch size OpenAi would allow


def get_embedding(memories, model="dlcru-text-embedding-ada-002"):
    # Set OpenAI configuration settings
    openai.api_type = "azure"
    openai.api_base = ""
    openai.api_version = ""
    openai.api_key = ""

    batches = [
        memories[i: i + MAX_BATCH_SIZE] for i in range(0, len(memories), MAX_BATCH_SIZE)
    ]
    embeddings = []
    for batch in batches:
        response = get_embeddings(batch, engine=model)
        embeddings.extend(np.array(response))
    return embeddings


class LongTermMemory:
    def __init__(
            self,
            similitude_metric="cosine",
    ):
        self.memories = []
        self.arrays = None

        if "dot" in similitude_metric:
            self.similitude_metric = dot_product
        elif "cosine" in similitude_metric:
            self.similitude_metric = cosine_similitude
        elif "adams" in similitude_metric:
            self.similitude_metric = adams_similitude

    def add_memory(self, memories, arrays=None):
        if not isinstance(memories, list):
            return self.add_single_memory(memories, arrays)
        self.add_memories(memories, arrays)

    def add_single_memory(self, memory, array=None):
        array = (
            array if array is not None else get_embedding([memory])[0]
        )
        if self.arrays is None:
            self.arrays = np.empty((0, len(array)), dtype=np.float32)
        elif len(array) != self.arrays.shape[1]:
            raise ValueError("All arrays must have the same length.")
        self.arrays = np.vstack([self.arrays, array]).astype(np.float32)
        self.memories.append(memory)

    def remove_memory(self, index):
        self.arrays = np.delete(self.arrays, index, axis=0)
        self.memories.pop(index)

    def add_memories(self, memories, arrays=None):
        if not memories:
            return
        arrays = arrays or np.array(get_embedding(memories)).astype(
            np.float32
        )

        # Check if arrays have consistent length
        if self.arrays is not None and arrays.shape[1] != self.arrays.shape[1]:
            raise ValueError("All arrays must have the same length.")

        # Create or extend the arrays array
        if self.arrays is None:
            self.arrays = arrays
        else:
            self.arrays = np.vstack([self.arrays, arrays])

        self.memories.extend(memories)

    def query(self, search_text, nr_docs=5, return_sim_scores=True):
        search_text_embeddings = get_embedding([search_text])[0]
        ranked_results, sim_scores = hyper_SVM_ranking_algorithm_sort(
            self.arrays, search_text_embeddings, nr_docs, self.similitude_metric)
        if return_sim_scores:
            return list(
                zip([self.memories[index] for index in ranked_results], sim_scores)
            )
        return [self.memories[index] for index in ranked_results]
