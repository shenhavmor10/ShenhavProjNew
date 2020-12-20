import React, { Component } from 'react'
import PropTypes from 'prop-types';


export class TodoItem extends Component {
    getStyle =() => {
        return {
            background : '#f4f4f4',
            padding: '10px',
            borderBottom: '1px #ccc dotted',
            textDecoration: this.props.todo.completed ? 'line-through' : 'none'
        }
    }

    
        
    

    render() {
        const { id,_id,title,completed} = this.props.todo;
        return (
            <div style={this.getStyle()}> 
                <p>
                    <input type='checkbox' value = {completed} checked={completed} onChange={this.props.markComplete.bind(this, id,this.props.todo)}/> 
                    {' '}
                    { title}
                    <button onClick={this.props.delTodo.bind(this,id,_id)} style={btnStyle}>x</button>
                </p>
            </div>
        )
    }
}

const btnStyle = {
    background: '#ff0000',
    color: '#fff',
    border: 'none',
    padding: '5px 8px',
    borederRadius: '50%',
    cursor: 'pointer',
    float: 'right'
}

//PropTypes
TodoItem.propTypes = {
    todos: PropTypes.object.isRequired
}



export default TodoItem
