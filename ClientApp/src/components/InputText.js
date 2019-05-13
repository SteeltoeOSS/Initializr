import React, { Component } from 'react';

export class InputText extends Component {
 
   render () {
    const {title, name, defaultValue, tabIndex } = this.props;
    return (
      <div className="control">
        <label>{title}</label>
        <input className="control-text" name={name} defaultValue={defaultValue} tabIndex={tabIndex} />
      </div>
    );
  }
}
