<template>
  <div id="todo">
    <section class="main" v-show="todos.length" v-cloak>
      <input id="toggle-all" class="toggle-all" type="checkbox" v-model="allDone" />
      <label for="toggle-all"></label>
      <ul class="todo-list">
        <li
          v-for="item in todos"
          class="todo"
          :key="item.id"
          :class="{ completed: todo.completed}"
        >
          <div class="view">
            <input class="toggle" type="checkbox" v-model="item.completed" />
            <label @dblclick="editTodo(todo)">{{ item.name }}</label>
            <button class="destroy" @click="removeTodo(todo)"></button>
          </div>
          <input class="edit" type="text" v-model="item.name" />
        </li>
      </ul>
    </section>
  </div>
</template>

<script>
import api from "../TodoListService";

export default {
  name: "Todo",
  props: {
     todos: Object
  },
  //MOVED TO APP.vue AS A TEST
  // data() {
  //   return {
  //    // todos: []
  //   };
  // },
  // methods: {
  //   async getTodos() {
  //     this.todos = await api.getTodos();
  //   }
  // }
};
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
