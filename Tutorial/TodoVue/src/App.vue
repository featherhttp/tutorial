<template>
  <div id="app">
    <header class="header">
      <h1>todos</h1>
      <input
        class="new-todo"
        autofocus
        autocomplete="off"
        placeholder="What needs to be done?"
        v-model="newTodo"
        @keyup.enter="createTodo(newTodo)"
      />
    </header>

    <TodoList todos="todos" />
  </div>
</template>

<script>
import TodoList from "../src/components/TodoList";
import api from "../src/TodoListService";

export default {
  name: "App",
  components: {
    TodoList,
  },
  data() {
    return {
      newTodo: "",
      todos: getTodos()
    };
  },
  methods: {
    async createTodo(newTodo) {
      let data = JSON.stringify({ name: newTodo })
      await api.createTodo(data);

      //these lines dont run ???
      console.log("create done")
      this.getTodos()
    },

    async getTodos() {
      // these are not running?
      console.log("in get todos")
      this.todos = await api.getTodos();
      console.log("ttods " + this.todos )
    }
  }
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
