import React, { Component } from 'react';
export class InputSelector extends Component {
    constructor(props) {
        super(props);
        
        this.handleChange = this.handleChange.bind(this);
        this.state = { selectedValue: this.props.defaultValue };
     //   console.log("in constructor ", this.props.name)
    }
    handleChange(e) {
        console.log('setting state from ' + this.state.selectedValue);

        var selection = e.target.attributes['data-value'].value
        this.setState({
            selectedValue: selection
        });
        this.props.onChange(this.props.name, selection)
        //console.log('to ' + this.selection);
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
                        <input type="hidden" name={this.props.name} value={this.state.selectedValue} />
                  
                    </div>
                </div>
            </div>
        );
    }
}
