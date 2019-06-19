import React, { Component } from 'react'
import { Typeahead } from 'react-bootstrap-typeahead'; 
import './DependenciesSelector.css'
import { DependencyViewSelector } from './DependencyViewSelector';

export class DependenciesSelector extends Component {
    #MAX_ITEMS = 5;
    constructor(props) {
        
        super(props);
        this.handleSelection = this.handleSelection.bind(this);
        this.handleViewChange = this.handleViewChange.bind(this);
        this.toggleHover = this.toggleHover.bind(this);
        this.state = {
            dependencies:[],
            selected_deps: [],
            activeView: 'quicksearch',
            hover: [false, false, false, false, false]
        }
        
    }
    componentDidMount() {
        fetch('/dependencies')
            .then(response => response.json())
            .then(data => {
                this.setState({
                    dependencies: data
                })
            })

    }
    handleViewChange(e) {
        this.setState({ activeView: e })
    }   

    handleSelection(currentSelection) {
        var nextDeps =  this.state.dependencies.map(x => {
            if (x.name == currentSelection.name) {
                x.selected = currentSelection.selected != true;
            }
            return x;
        });
        this.setState({
            dependencies: nextDeps

        })
        if (this._typeAhead) {
            this._typeAhead._instance.clear();
        }

    }
    toggleHover(index) {
    var newState = new Array(this.#MAX_ITEMS);
        newState[index] = true;
        this.setState({ hover: newState });
    }

    _renderMenuItemChildren = (option, props, index) => {

        return [
            <div className="dependencies-list">
                <a className={'dependency-item dependency-item-gray ' + (this.state.hover[index] ? 'selected' : '')} onMouseEnter={() => this.toggleHover(index)} href="/">
                    <strong className="title">
                        {option.name}
                    </strong>
                    <br />
                    <span className="description" key={index} className="desc">
                        {option.description}
                    </span>
                    <span className="icon"><svg aria-hidden="true" focusable="false" data-prefix="fas" data-icon="plus" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512" className="icon-plus"><path fill="currentColor" d="M416 208H272V64c0-17.67-14.33-32-32-32h-32c-17.67 0-32 14.33-32 32v144H32c-17.67 0-32 14.33-32 32v32c0 17.67 14.33 32 32 32h144v144c0 17.67 14.33 32 32 32h32c17.67 0 32-14.33 32-32V304h144c17.67 0 32-14.33 32-32v-32c0-17.67-14.33-32-32-32z"></path></svg></span>
                </a>
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
                    <input name="dependencies" type="hidden" defaultValue={this.state.dependencies.filter(d => d.selected === true).map(d => d.shortName)} />

                    {this.state.activeView == 'quicksearch' &&
                        <div className="colset">
                            <div className="col">Search dependencies to add
                            <div className="control">
                                    <Typeahead
                                    id="typeahead"
                                    className={'control-text'}
                                    labelKey="name"
                                    maxHeight={'500px'}
                                    maxResults={4}
                                    paginationText={'More than 4 results found. Refine your search if necessary'}
                                    multiple={true}
                                    options={this.state.dependencies.filter(x => !x.selected)}
                                    renderMenuItemChildren={this._renderMenuItemChildren}
                                    placeholder="Circuit Breaker, Actuator ..."
                                    onChange={(e) => { this.handleSelection(e[0]) }}
                                    ref={(ref) => this._typeAhead = ref}
                                />
                                </div>
                            </div>
                            <div className="col" id="col-dep" >
                               Selected dependencies
                            <div className="dependencies-list dependencies-list-checked" id="list-added">
                               
                                {
                                    this.state.dependencies.filter(d => d.selected === true).map((item) => {
                                        return <a className="dependency-item checked" onClick={() => { this.handleSelection(item) }}>
                                                <div>
                                                    <strong> {item.name} </strong>
                                                    <br/>
                                                <span className="description"> {item.description} </span>
                                                    
                                                <span className="icon"><svg aria-hidden="true" focusable="false" data-prefix="fas" data-icon="times" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 352 512" className="icon-times"><path fill="currentColor" d="M242.72 256l100.07-100.07c12.28-12.28 12.28-32.19 0-44.48l-22.24-22.24c-12.28-12.28-32.19-12.28-44.48 0L176 189.28 75.93 89.21c-12.28-12.28-32.19-12.28-44.48 0L9.21 111.45c-12.28 12.28-12.28 32.19 0 44.48L109.28 256 9.21 356.07c-12.28 12.28-12.28 32.19 0 44.48l22.24 22.24c12.28 12.28 32.2 12.28 44.48 0L176 322.72l100.07 100.07c12.28 12.28 32.2 12.28 44.48 0l22.24-22.24c12.28-12.28 12.28-32.19 0-44.48L242.72 256z"></path></svg><svg aria-hidden="true" focusable="false" data-icon="check" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" className="icon-check"><path fill="currentColor" d="M173.898 439.404l-166.4-166.4c-9.997-9.997-9.997-26.206 0-36.204l36.203-36.204c9.997-9.998 26.207-9.998 36.204 0L192 312.69 432.095 72.596c9.997-9.997 26.207-9.997 36.204 0l36.203 36.204c9.997 9.997 9.997 26.206 0 36.204l-294.4 294.401c-9.998 9.997-26.207 9.997-36.204-.001z"></path></svg></span>
                                                </div>
                                                </a>
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
                                        return <a className={option.selected == true ? 'checked' : ''} onClick={() => { this.handleSelection(option) }} >
                                            <div>
                                                <strong>{option.name}</strong><br />
                                                <span>{option.description}</span>
                                                <span className="icon"><svg aria-hidden="true" focusable="false" data-prefix="fas" data-icon="plus" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512" className="icon-plus">

                                                    <path fill="currentColor" d="M416 208H272V64c0-17.67-14.33-32-32-32h-32c-17.67 0-32 14.33-32 32v144H32c-17.67 0-32 14.33-32 32v32c0 17.67 14.33 32 32 32h144v144c0 17.67 14.33 32 32 32h32c17.67 0 32-14.33 32-32V304h144c17.67 0 32-14.33 32-32v-32c0-17.67-14.33-32-32-32z"></path></svg><svg aria-hidden="true" focusable="false" data-prefix="fas" data-icon="times" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 352 512" className="icon-times"><path fill="currentColor" d="M242.72 256l100.07-100.07c12.28-12.28 12.28-32.19 0-44.48l-22.24-22.24c-12.28-12.28-32.19-12.28-44.48 0L176 189.28 75.93 89.21c-12.28-12.28-32.19-12.28-44.48 0L9.21 111.45c-12.28 12.28-12.28 32.19 0 44.48L109.28 256 9.21 356.07c-12.28 12.28-12.28 32.19 0 44.48l22.24 22.24c12.28 12.28 32.2 12.28 44.48 0L176 322.72l100.07 100.07c12.28 12.28 32.2 12.28 44.48 0l22.24-22.24c12.28-12.28 12.28-32.19 0-44.48L242.72 256z"></path></svg><svg aria-hidden="true" focusable="false" data-icon="check" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" className="icon-check"><path fill="currentColor" d="M173.898 439.404l-166.4-166.4c-9.997-9.997-9.997-26.206 0-36.204l36.203-36.204c9.997-9.998 26.207-9.998 36.204 0L192 312.69 432.095 72.596c9.997-9.997 26.207-9.997 36.204 0l36.203 36.204c9.997 9.997 9.997 26.206 0 36.204l-294.4 294.401c-9.998 9.997-26.207 9.997-36.204-.001z"></path>
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
