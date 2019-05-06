import React, { Component } from 'react'; 
import 'react-bootstrap-typeahead/css/Typeahead-bs4.css';
import { FormRow } from './FormRow';
import { Typeahead } from 'react-bootstrap-typeahead'
import './Home.css'
const hidden = {
    display: "none"
}
export class Home extends Component {
    static displayName = Home.name;
    constructor(props) {
        super(props);
        this.state = {
            project: 'gradle-project',
            dependencies: ['Hystrix', 'Actuator', 'MySql', 'Serilog']
        }
        this.handleChange = this.handleChange.bind(this);
    }
    handleChange(e) {

        console.log('setting state from ' + this.state.project );
        this.setState({
            project: e.target.attributes['data-value'].value
        });
        console.log('to ' + this.state.project);
    }


  render () {
    return (
        <div>
            <form name="form" action="/starter.zip" method="get" autocomplete="off">
                <input type="text" style={hidden} name="fakeusernameremembered" />
                <input type="password" style={hidden} name="fakepasswordremembered" />
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
<div class="control"><input id="inputSearch" name="inputSearch" class="control-text" autocomplete="disabled" type="text" tabindex="3" placeholder="Web, Security, JPA, Actuator, Devtools..." value="" />
</div>
                                                                                        <div class="no-result" id="noresult-to-add"><em>No result.</em></div>
                                                                                        <div class="list" id="list-to-add"></div>
                            </div>
                            <Typeahead
                                labelKey="dependencies"
                                multiple={true}
                                options={this.state.dependencies}
                                placeholder="Actuators, Hystrix, etc"
                            />
                                                                                    <div class="col hide" id="col-dep">
                                                                                        <strong>Selected dependencies</strong>
                                                                                        <div class="list light" id="list-added"></div>
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                        </div>
                                                                        <div class="line row-action">
                                                                            <div class="left">
                                                                                <div class="footer">© 2013-2019 Pivotal Software<br/>start.spring.io is powered by<br/><a href="https://github.com/spring-io/initializr/" tabindex="998">Spring Initializr</a> and <a href="https://run.pivotal.io/" tabindex="999">Pivotal Web Services</a></div>
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

        