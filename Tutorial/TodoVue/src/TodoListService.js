import Vue from 'vue'
import axios from 'axios'

const client = axios.create({
  baseURL: 'https://localhost:5001/api/todos',
  json: true
})

export default {
    async execute(method, resource, data) {
      return client({
        method,
        url: resource,
        data,
      }).then(req => {
        return req.data
      })
    },
    getTodos() {
      return this.execute('get', '/')
    },
    createTodo(data) {
      return this.execute('post', '/', data)
    },
    updateCompleted(id, data) {
      return this.execute('put', `/${id}`, data)
    },
    deleteTodo(id) {
      return this.execute('delete', `/${id}`)
    }
  }