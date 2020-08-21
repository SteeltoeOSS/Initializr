import React, { Component } from 'react';
export class InputSelector extends Component {
    constructor(props) {
        super(props);

        this.handleChange = this.handleChange.bind(this);
        //this.state = { selectedValue: this.props.defaultValue };

    }
    handleChange(e) {
       this.props.onChange(this.props.name, e.target.attributes['data-value'].value);
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
                                return <div key={ 'option'+i }  className={'radio ' + (this.props.selectedValue === item ? 'active' : '')} onClick={this.handleChange} >
                                    <a href={hrefLink} data-value={item}>{item}</a>
                                    </div>
                            })
                        }
                        <input type="hidden" name={this.props.name} value={this.props.selectedValue} />
                    </div>
                    <span  style={{display: this.props.invalidText? "block": "none" , "color": "red"}} > {this.props.invalidText} </span>
                </div>
            </div>
        );
    }
}
