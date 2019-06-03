import React, { Component } from 'react'
import { Typeahead } from 'react-bootstrap-typeahead'; 
export class AutoSuggestSelector extends Component {
    constructor(props) {
        super(props);
        this.handleSelection = this.handleSelection.bind(this);
        console.log('in constructor AutoSuggest');
        this.state = {
            dependencies:[],
            selected_deps: []
        }
        
    }
    componentDidMount() {
        fetch('/dependencies')
            .then(response => response.json())
            .then(data => {
                this.setState({ dependencies: data })
                console.log('data returned', data, this.state.dependencies)
            })

    }
       
    handleSelection(e) {
        var currentSelection = e[0];
        var selected_deps = [...this.state.selected_deps, currentSelection];
        this.setState(prevState => ({
            selected_deps: selected_deps,
            dependencies: [...prevState.dependencies.filter(d => !selected_deps.some(sd => sd.name == d.name))]

        }))
        this._typeAhead._instance.clear();

        console.log('childn state', selected_deps, this.state.selected_deps)
    }
    removeDependency(dependency) {
        var selected_deps = [...this.state.selected_deps.filter(d => d.name != dependency.name)]
        this.setState(prevState => ({
            selected_deps: selected_deps,
            dependencies: [...prevState.dependencies, dependency].sort((a, b) => a.name > b.name)
        }))
    }
    _renderMenuItemChildren = (option, props, index) => {
         return [
            <div className="title">
                {option.name}
            </div>,
            <div key={index} className="desc">
                    {option.description}
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
                                <input name="dependencies" type="hidden" defaultValue={this.state.selected_deps.map(d => d.name)} />
                                                      
                                {
                                    this.state.selected_deps.map((item) => {
                                        return <div className="item" >
                                                    <div className="title"> {item.name} </div>
                                            <div className="description"> {item.description} </div>
                                            <a class="btn-ico" onClick={() => { this.removeDependency(item) } }><i class="fas fa-times"></i></a>
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
