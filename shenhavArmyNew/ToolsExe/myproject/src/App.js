import React, { Component } from 'react';
import { BrowserRouter as Router, Route} from "react-router-dom";
import Header from './components/layout/Header';
import Todos from './components/Todos';
import AddTodo from './components/AddTodo';
import About from './components/pages/About';
import axios from 'axios';
import './App.css';


class App extends Component {
  state = {
    idGenerator: 0,
    todos: [
      
    ]
    
  }

  componentDidMount=()=>{
    this.GetInitialData()
  }

  GetInitialData = async() => {
    const homePage = await axios.get('http://localhost:3007/todos')
    console.log(homePage)
    this.setState({todos: homePage.data, idGenerator: homePage.data.length+1});
  }

  markComplete =(id,todo)=>{
    this.setState( { todos: this.state.todos.map(todo => {
      if(todo.id === id){
        todo.completed = !todo.completed
      }
      return todo;

    })} );
    this.UpdateTodoCheckbox(todo)
  }

  delTodo = (id,_id) =>{
    this.setState({ todos: [...this.state.todos.filter(todo => todo.id !== id)]});
    this.DeleteTodoFromServer(_id);


  }

  AddTodoToServer = async (todo) => {
    await axios.post("http://localhost:3007/todos/",todo);
    this.GetInitialData();
  }

  DeleteTodoFromServer = async (_id)=> {
    await axios.delete("http://localhost:3007/todos/"+_id)
  }

  UpdateTodoCheckbox = async (todo)=> {
    await axios.patch("http://localhost:3007/todos/"+todo._id,{completed:todo.completed})

  }
  addTodo =(title)=> {
    const newTodo = {
      id:this.state.idGenerator,
      title,
      completed: false
    }
    this.setState({idGenerator: this.state.idGenerator+1,todos: [...this.state.todos,newTodo]})
    this.AddTodoToServer(newTodo)
  }
  
  render() {
    return (
      <Router>
        <div className="App">
          <div className="container">
          <Header />
          <Route exact path="/" render={props=>(
            <React.Fragment>
              <AddTodo addTodo={this.addTodo}/>
              <Todos todos={this.state.todos} markComplete={this.markComplete} 
              delTodo ={this.delTodo}/>
            </React.Fragment>
          )} />
          <Route path="/about" component = {About}/>
          
          </div>
          
        </div>
      </Router>
    );
  }
}

export default App;
