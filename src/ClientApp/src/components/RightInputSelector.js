import React, { Component } from 'react';
export class RightInputSelector extends Component {
    constructor(props) {
        super(props);
        this.handleChange = this.handleChange.bind(this);
        this.state = { selectedValue: props.defaultValue };

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
            <div className="control"><label>{this.props.title}</label>
            <div className="radios">
                        {
                            this.props.values.map((item, i ) => {
                                return <div id={"rdio" + i} key= { "rdio"+i }  className={'radio ' + (this.state.selectedValue === item ? 'active' : '')} onClick={this.handleChange}>
                                            <a href={hrefLink} data-value={item}>{item}</a>
                                        </div>
                            })
                        }
                   
                    </div>
                </div>
        );
    }
}
