<!-- Template: define o HTML que será renderizado -->
<template>
  <div>
    <h1>Minhas Tarefas</h1>

    <!-- v-model faz o two-way binding: o valor do input fica sincronizado
         com a variável reativa "tituloTarefa" -->
    <input v-model="tituloTarefa" placeholder="Título da Tarefa">
    <textarea v-model="descricaoTarefa" placeholder="Descrição"></textarea>

    <!-- @click é o atalho de v-on:click — chama a função ao clicar -->
    <button @click="adicionarTarefa">Adicionar</button>

    <!-- v-for renderiza um <li> para cada item da lista "tarefas"
         :key é obrigatório — ajuda o Vue a identificar cada elemento -->
    <ul>
      <li v-for="item in tarefas" :key="item.id">
        <!-- Operador ternário: se concluida for true mostra "Concluida" -->
        {{ item.titulo }} - {{ item.concluida ? "Concluida" : "Aguardando" }}
        <button @click="concluirTarefa(item)">Concluir</button>
        <button @click="deletarTarefa(item.id)">Deletar</button>
      </li>
    </ul>
  </div>
</template>

<!-- Script: lógica do componente usando Composition API -->
<script setup>
  // ref() cria variáveis reativas — quando mudam, o template atualiza sozinho
  // onMounted() executa código quando o componente é inserido no DOM
  import { ref, onMounted } from "vue";

  // Axios é a biblioteca para fazer requisições HTTP
  import axios from "axios";

  // Lista reativa de tarefas — começa vazia
  const tarefas = ref([]);

  // Variáveis reativas ligadas aos campos do formulário via v-model
  const tituloTarefa = ref("");
  const descricaoTarefa = ref("");

  // URL base da API — deve bater com a porta do backend
  const API_URL = "http://localhost:5204/api/tarefas";

  // Busca todas as tarefas da API (GET) e armazena na variável reativa
  const buscarTarefas = async () => {
    const resposta = await axios.get(API_URL);
    tarefas.value = resposta.data; // .value é necessário para acessar ref()
  };

  // Envia uma nova tarefa para a API (POST) e recarrega a lista
  const adicionarTarefa = async () => {
    // Validação: não envia se os dois campos estiverem vazios
    if (!tituloTarefa.value && !descricaoTarefa.value) return;

    // Envia o objeto como JSON no corpo da requisição
    await axios.post(API_URL, {
      titulo: tituloTarefa.value,
      descricao: descricaoTarefa.value,
      concluida: false // ⚠️ Não bate com o campo "Cancelado" do Model C#
    });

    // Limpa os campos após adicionar
    tituloTarefa.value = "";
    descricaoTarefa.value = "";

    // Recarrega a lista para mostrar a nova tarefa
    buscarTarefas();
  };

  // Executa buscarTarefas() assim que o componente for montado na página
  onMounted(buscarTarefas);

  // Funções extras para deletar e concluir tarefas — não estão no template, mas podem ser usadas depois
  // Deleta uma tarefa chamando DELETE /api/tarefas/{id}
  const deletarTarefa = async (id) => {
    await axios.delete(`${API_URL}/${id}`);
    buscarTarefas(); // recarrega a lista
  };

  // Marca como concluída chamando PUT /api/tarefas/{id}
  const concluirTarefa = async (item) => {
    await axios.put(`${API_URL}/${item.id}`, {
      ...item,           // spread: copia todos os campos do item
      concluida: true    // só altera o campo concluida
    });
    buscarTarefas();
  };
</script>