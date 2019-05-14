import React, { Component } from 'react';
import { Typeahead } from 'react-bootstrap-typeahead'; 
export class AutoSuggestSelector extends Component {
    constructor(props) {
        super(props);
        this.handleSelection = this.handleSelection.bind(this);
        this.state = {
            dependencies: this.props.available_deps,
            selected_deps: []
        }
        
    }
       
    handleSelection(e) {
        //console.log("selection " + e);
        //this.state.selected_deps.push(e);
        var deps = [...this.state.selected_deps, e] 
        this.setState(prevState => ({ selected_deps: [...prevState.selected_deps, e ]}))
        this._typeAhead._instance.clear();

        console.log('childn state', deps, e)
      //  this.props.onChange(this.props.id, deps);

       // console.log('typeahead' , this._typeAhead);

    }
    _renderMenuItemChildren = (option, props, index) => {
       // console.log(props)
        return [
           <div className="title">
                {option}
            </div>,
            <div key={index} className="desc">
                    {'This is the description for ' + option}
              </div>
        ];
    }


    render() {
        let hrefLink = '#';
        return (
            <div className="line">
                <div className="left">
                    <div className="dependencies-label">Dependencies</div>
                </div>
                <div className="right">
                    <div className="colset">
                        <div className="col">Search dependencies to add
                            <div className="control">
                                <Typeahead
                                id="typeahead"
                                className={'control-text'}
                                labelKey="name"
                                multiple={true}
                                options={this.state.dependencies}
                                renderMenuItemChildren={this._renderMenuItemChildren}
                                placeholder="Hystrix, Actuator ..."
                                onChange={this.handleSelection}
                                ref={(ref) => this._typeAhead = ref} />
                            </div>           
                        </div>
                        <div className="col" id="col-dep">
                            <strong>Selected dependencies</strong>
                             <div className="list light" id="list-added">
                             <input name="dependencies" type="hidden"  defaultValue={this.state.selected_deps}  />
                                                      
                                {
                                    this.state.selected_deps.map((item) => {
                                        return <div className="item" >
                                                    <div className="title"> {item} </div>
                                                    <div className="description"> Some description about {item} </div>
                                                </div>
                                        })

                                }
                            </div>
                        </div>
                    </div>
                </div>
            </div>

        );
    }
}
