<template>
  <div class="container">
    <h1>Minhas Tarefas</h1>

    <div class="input-row">
      <input
        v-model="novaTarefa"
        placeholder="Digite uma nova tarefa..."
        @keyup.enter="adicionarTarefa"
      />
      <button @click="adicionarTarefa">Adicionar</button>
    </div>

    <ul>
      <li v-for="tarefa in tarefas" :key="tarefa.id">
        <span v-if="editandoId !== tarefa.id">
          {{ tarefa.titulo }}
        </span>
        <input
          v-else
          v-model="tituloEditado"
          @keyup.enter="salvarEdicao(tarefa)"
        />
        <div class="acoes">
          <button
            v-if="editandoId !== tarefa.id"
            class="btn-editar"
            @click="iniciarEdicao(tarefa)"
          >Editar</button>
          <button
            v-else
            class="btn-salvar"
            @click="salvarEdicao(tarefa)"
          >Salvar</button>
          <button
            class="btn-excluir"
            @click="excluirTarefa(tarefa.id)"
          >Excluir</button>
        </div>
      </li>
    </ul>

    <p v-if="tarefas.length === 0" class="vazio">
      Nenhuma tarefa ainda. Adicione uma acima!
    </p>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'

// Troque pela porta da sua API
const API = 'http://localhost:5152/api/tarefas'

const tarefas = ref([])
const novaTarefa = ref('')
const editandoId = ref(null)
const tituloEditado = ref('')

async function carregarTarefas() {
  const res = await axios.get(API)
  tarefas.value = res.data
}

async function adicionarTarefa() {
  if (!novaTarefa.value.trim()) return
  await axios.post(API, { titulo: novaTarefa.value })
  novaTarefa.value = ''
  carregarTarefas()
}

function iniciarEdicao(tarefa) {
  editandoId.value = tarefa.id
  tituloEditado.value = tarefa.titulo
}

async function salvarEdicao(tarefa) {
  await axios.put(`${API}/${tarefa.id}`, {
    id: tarefa.id,
    titulo: tituloEditado.value,
    concluida: tarefa.concluida
  })
  editandoId.value = null
  carregarTarefas()
}

async function excluirTarefa(id) {
  await axios.delete(`${API}/${id}`)
  carregarTarefas()
}

onMounted(carregarTarefas)
</script>

<style scoped>
.container{
    max-width:600px;
    margin:2rem auto;
    padding:0 1rem;
    font-family:sans-serif
}

h1{
    font-size:1.5rem;
    margin-bottom:1.25rem
}

.input-row{
    display:flex;
    gap:8px;
    margin-bottom:1.25rem
}

.input-row input{
    flex:1;
    padding:8px 12px;
    border:1px solid #ccc;
    border-radius:6px;
    font-size:14px
}

.input-row button{
    padding:8px 16px;
    background:#534AB7;
    color:white;
    border:none;
    border-radius:6px;
    cursor:pointer
}

ul{
    list-style:none;
    padding:0
}

li{
    display:flex;
    align-items:center;
    justify-content:space-between;
    padding:10px 12px;
    border:1px solid #e5e5e5;
    border-radius:6px;
    margin-bottom:6px;
    gap:8px
}

li span{
    flex:1;
    font-size:14px
}

li input{
    flex:1;
    padding:4px 8px;
    border:1px solid #ccc;
    border-radius:4px;
    font-size:14px
}

.acoes{
    display:flex;
    gap:6px
}

.btn-editar{
    padding:4px 10px;
    background:#EF9F27;
    color:white;
    border:none;
    border-radius:4px;
    cursor:pointer;
    font-size:12px
}

.btn-salvar{
    padding:4px 10px;
    background:#3B6D11;
    color:white;
    border:none;
    border-radius:4px;
    cursor:pointer;
    font-size:12px
}

.btn-excluir
{padding:4px 10px;
    background:#E24B4A;
    color:white;
    border:none;
    border-radius:4px;
    cursor:pointer;
    font-size:12px
}

.vazio{
    color:#888;
    font-size:14px;
    text-align:center;
    margin-top:2rem
}

</style>