﻿import React, { Component } from 'react';
export class RightInputSelector extends Component {
    constructor(props) {
        super(props);
        this.handleChange = this.handleChange.bind(this);
      //  this.state = { selectedValue: props.defaultValue };

    }
    handleChange(e) {
        this.props.onChange(this.props.name, e.target.attributes['data-value'].value)
    }


    render() {
        let hrefLink = '#';
        return (
            <div className="control"><label>{this.props.title}</label>
            <div className="radios">
                        {
                            this.props.values.map((item, i ) => {
                                return <div id={"rdio" + i} key= { "rdio"+i }  className={'radio ' + (this.props.selectedValue === item ? 'active' : '')} onClick={this.handleChange}>
                                            <a href={hrefLink} data-value={item}>{item}</a>
                                        </div>
                            })
                    }
                    <input type="hidden" name={this.props.name} value={this.props.selectedValue} />
                    </div>
                </div>
        );
    }
}
