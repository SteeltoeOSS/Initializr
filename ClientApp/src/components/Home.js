import React, { Component } from 'react'; 
import { FormRow } from './FormRow';
//import { Suggestions } from './Suggestions';
import { Highlighter, Typeahead } from 'react-bootstrap-typeahead'; 
import './Home.css'

const hidden = {
    display: "none"
}
const border = {
    borderBottom: "3px solid #e4e4e4",
    lineHeight: "16px",
    padding: ".35rem 0 .55rem"
}
export class Home extends Component {
    static displayName = Home.name;
    constructor(props) {
        super(props);
        this.state = {
            project: 'gradle-project',
            dependencies: ['Hystrix', 'Actuator', 'MySql', 'Serilog'],
            selected_deps: []
        }
        this.handleChange = this.handleChange.bind(this);
        this.handleSelection = this.handleSelection.bind(this);
    }
    handleChange(e) {

        console.log('setting state from ' + this.state.project );
        this.setState({
            project: e.target.attributes['data-value'].value
        });
        console.log('to ' + this.state.project);
    }
    handleSelection(e) {
        console.log("selection " + e);
        //this.state.selected_deps.push(e);
        this.setState(prevState => ({ selected_deps: [...prevState.selected_deps, e ]}))
        this._typeahead._instance.clear();

        console.log('typeahead' , this._typeahead);

    }
    _renderMenuItemChildren = (option, props, index) => {
        console.log(props)
        return [
           <div className="title">
                {option}
            </div>,
            <div className="desc">
                    {'This is the description for ' + option}
              </div>
        ];
    }


  render () {
    return (
        <div>
            <form name="form" action="/starter.zip" method="post"  autocomplete="off">
                <input type="text" style={hidden} name="fakeusernameremembered" />
                <input type="password" style={hidden} name="fakepasswordremembered" />
                <input type="text" style={hidden} name="type" value="Console" />
                <FormRow title='Project' values={ ["Visual Studio"]}/>
                <FormRow title='Language' values={["C#", "F#", "VB.NET"]} />
                <FormRow title='Steeltoe' values={["2.1", "2.2", "3.0"]} />
                                 <div class="line">
                                        <div class="left">Project Metadata</div>
                                        <div class="right">
                                            <div class="project-metadata">
                                                <input id="baseDir" name="baseDir" type="hidden" value="demo" />
                                                    <div class="control"><label>Project Name</label>
                                                        <input class="control-text" name="projectName" value="SteeltoeExample" tabindex="1" />
</div>
                                                             <div class="more-block">
                                                                <div id="more-block">
                                                                    <div class="control"><label>Name</label>
                                                                        <input class="control-text" name="name" value="demo" />
</div>
                                                                        <div class="control"><label>Description</label>
                                                                            <input class="control-text" name="description" value="Demo project for Steeltoe" />
</div>
                                                 
                                                                                    <div class="control"><label>.NET Core Version</label>
                                                                                        <div class="radios">
                                                                                            <div class="radio active">
                                                                                                <a href="#" data-value="2.2">2.2</a>
                                                                                            </div>
                                                                                            <div class="radio">
                                                                                                <a href="#" data-value="3.0">3.0</a>
                                                                                            </div>
                                                                                          
                                                                                            <input type="hidden" name="netCoreVersuib" value="2.2" />
</div>
                                                                                        </div>
                                                                                    </div>
                                                                                </div>
                                                                                <div class="control control-submit">
                                                                                    <p id="more-options">
                                                                                        <button type="button" class="btn">More options</button>
                                                                                    </p>
                                                                                    <p id="fewer-options" class="hide">
                                                                                        <button type="button" class="btn">Fewer options</button>
                                                                                    </p>
                                                                                </div>
                                                                            </div>
                                                                        </div>
                                                                    </div>
                                                                    <div class="line">
                                                                        <div class="left">
                                                                            <div class="dependencies-label">Dependencies <a id="see-all">See all</a></div>
                                                                        </div>
                                                                        <div class="right">
                                                                            <div class="colset">
                                                                                <div class="col">Search dependencies to add
                                <div class="control">
                                    <Typeahead
                                        className={'control-text'}
                                        labelKey="name"
                                        multiple={true}
                                        options={this.state.dependencies}
                                        renderMenuItemChildren={this._renderMenuItemChildren}
                                        placeholder="Hystrix, Actuator ..."
                                        onChange={this.handleSelection}
                                        ref={(ref) => this._typeahead = ref}
                                    />
</div>
                                                                                        <div class="no-result" id="noresult-to-add"><em>No result.</em></div>
                                <div class="list" id="list-to-add">
                                  
                                </div>
                            </div>
                          
                                                                                    <div class="col" id="col-dep">
                                                                                        <strong>Selected dependencies</strong>
                                <div class="list light" id="list-added">
                                    {
                                        this.state.selected_deps.map((item) => {
                                            return <div class="item" >
                                                <div class="title">
                                                    {item}
                                                </div>
                                                <div class="description">
                                                    Some description about {item}
                                                </div>
                                            </div>
                                        })

                                    }
                                </div>
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                        </div>
                                                                        <div class="line row-action">
                                                                            <div class="left">
                                                                                <div class="footer">© 2013-2019 Pivotal Software<br/>start.steeltoe.io is powered by<br/>  <a href="">.NET foundation</a> and <a href="https://run.pivotal.io/" tabindex="999">Pivotal Web Services</a></div>
</div>
                                                                                    <div class="right">
                                                                                        <div class="submit">
                                                                                            <button id="btn-generate" type="submit" class="btn btn-primary" tabindex="4">
                                                                                                <span class="text">Generate Project</span> -
<span class="shortcut"> alt + ⏎</span>
                                                                                            </button>
                                                                                        </div>
                                                                                    </div>
                                                                                </div>
</form>
   </div>

    );
  }
}

        