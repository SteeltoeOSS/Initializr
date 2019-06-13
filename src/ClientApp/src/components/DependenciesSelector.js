import React, { Component } from 'react'
import { Typeahead } from 'react-bootstrap-typeahead'; 
import './DependenciesSelector.css'
import { DependencyViewSelector } from './DependencyViewSelector';

export class DependenciesSelector extends Component {
    constructor(props) {
        super(props);
        this.handleSelection = this.handleSelection.bind(this);
        this.handleViewChange = this.handleViewChange.bind(this);
        console.log('in constructor AutoSuggest');
        this.state = {
            dependencies:[],
            selected_deps: [],
            activeView: 'quicksearch'
        }
        
    }
    componentDidMount() {
        fetch('/dependencies')
            .then(response => response.json())
            .then(data => {
                this.setState({
                    dependencies: data
                })
                //console.log('data returned', data, this.state.dependencies)
            })

    }
    handleViewChange(e) {
        console.log(e);
        this.setState({ activeView: e })
    }   

    handleSelection(e) {
        var currentSelection = e[0];
        var selected_deps = [...this.state.selected_deps, currentSelection];
        this.setState(prevState => ({
            selected_deps: selected_deps,
            dependencies: [...prevState.dependencies.filter(d => !selected_deps.some(sd => sd.name == d.name))]

        }))
        this._typeAhead._instance.clear();

        //console.log('child state', selected_deps, this.state.selected_deps)
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
                <div className="dependencies-box">
                    <DependencyViewSelector onChange={this.handleViewChange} />
                    {this.state.activeView == 'quicksearch' &&
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
                            <div className="col" id="col-dep" >
                                <strong>Selected dependencies</strong>
                                <div className="selectedDeps light" id="list-added">
                                    <input name="dependencies" type="hidden" defaultValue={this.state.selected_deps.map(d => d.name)} />

                                    {
                                        this.state.selected_deps.map((item) => {
                                            return <div className="item" >
                                                <div className="title"> {item.name} </div>
                                                <div className="description"> {item.description} </div>
                                                <a class="btn-ico" onClick={() => { this.removeDependency(item) }}><i class="fas fa-times"></i></a>
                                            </div>
                                        })

                                    }
                                </div>
                            </div>
                        </div>
                    }
                    {this.state.activeView == 'list' &&
                        <div className="groups">
                            <div className="group">
                                <div className="group-items">
                                {
                                    this.state.dependencies.map((option, index) => {
                                        return <a className="">
                                            <div>
                                                <strong>{option.name}</strong><br />
                                                <span>{option.description}</span>
                                                <span className="icon"><svg aria-hidden="true" focusable="false" data-prefix="fas" data-icon="plus" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512" className="icon-plus">

                                                    <path fill="currentColor" d="M416 208H272V64c0-17.67-14.33-32-32-32h-32c-17.67 0-32 14.33-32 32v144H32c-17.67 0-32 14.33-32 32v32c0 17.67 14.33 32 32 32h144v144c0 17.67 14.33 32 32 32h32c17.67 0 32-14.33 32-32V304h144c17.67 0 32-14.33 32-32v-32c0-17.67-14.33-32-32-32z"></path></svg><svg aria-hidden="true" focusable="false" data-prefix="fas" data-icon="times" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 352 512" class="icon-times"><path fill="currentColor" d="M242.72 256l100.07-100.07c12.28-12.28 12.28-32.19 0-44.48l-22.24-22.24c-12.28-12.28-32.19-12.28-44.48 0L176 189.28 75.93 89.21c-12.28-12.28-32.19-12.28-44.48 0L9.21 111.45c-12.28 12.28-12.28 32.19 0 44.48L109.28 256 9.21 356.07c-12.28 12.28-12.28 32.19 0 44.48l22.24 22.24c12.28 12.28 32.2 12.28 44.48 0L176 322.72l100.07 100.07c12.28 12.28 32.2 12.28 44.48 0l22.24-22.24c12.28-12.28 12.28-32.19 0-44.48L242.72 256z"></path></svg><svg aria-hidden="true" focusable="false" data-icon="check" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" class="icon-check"><path fill="currentColor" d="M173.898 439.404l-166.4-166.4c-9.997-9.997-9.997-26.206 0-36.204l36.203-36.204c9.997-9.998 26.207-9.998 36.204 0L192 312.69 432.095 72.596c9.997-9.997 26.207-9.997 36.204 0l36.203 36.204c9.997 9.997 9.997 26.206 0 36.204l-294.4 294.401c-9.998 9.997-26.207 9.997-36.204-.001z"></path>
                                                    </svg>
                                                </span></div>
                                        </a>
                                    })
                                }
                                
                                </div>
                            </div>
                        </div>
                    }   
                </div>
            </div>

        );
    }
}
