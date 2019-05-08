import React, { Component } from 'react';
export class InputSelector extends Component {
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
        let hrefLink = '#';
        return (
            <div className="line">
                <div className="left">{this.props.title}</div>
                <div className="right">
                    <div className="radios">
                        {
                            this.props.values.map((item, i ) => {
                                return <div key={ 'option'+i }  className={'radio ' + (this.state.selectedValue === item ? 'active' : '')} onClick={this.handleChange} >
                                    <a href={hrefLink} data-value={item}>{item}</a>
                                </div>
                            })
                        }
                   
                    </div>
                </div>
            </div>
        );
    }
}
