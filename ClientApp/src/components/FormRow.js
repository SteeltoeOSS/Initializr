import React, { Component } from 'react';
export class FormRow extends Component {
    //    static displayName = Home.name;
    constructor(props) {
        super(props);
        this.handleChange = this.handleChange.bind(this);
        this.state = { selectedValue: props.values[0] };
    }
    handleChange(e) {
        console.log('setting state from ' + this.state.selectedValue);
        this.setState({
            selectedValue: e.target.attributes['data-value'].value
        });
        console.log('to ' + this.state.selectedValue);
    }


    render() {
        return (
            <div class="line">
                <div class="left">{this.props.title}</div>
                <div class="right">
                    <div class="radios">
                        {
                            this.props.values.map((item, i ) => {
                                return <div class={'radio ' + (this.state.selectedValue == item ? 'active' : '')} onClick={this.handleChange} >
                                    <a href="#" data-value={item}>{item}</a>
                                </div>
                            })
                        }
                   
                    </div>
                </div>
            </div>
        );
    }
}
