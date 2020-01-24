import React, { Component } from 'react';

export class InputText extends Component {
 
   render () {
    const {title, ...inputProps} = this.props;
    return (
      <div className="control">
        <label>{title}</label>
            <input className="control-text" {...inputProps} />
      </div>
    );
  }
}
