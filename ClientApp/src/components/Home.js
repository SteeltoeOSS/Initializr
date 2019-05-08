import React, { Component } from 'react'; 
import { InputSelector } from './InputSelector';
import { RightInputSelector } from './RightInputSelector';
//import { Suggestions } from './Suggestions';
import { Typeahead } from 'react-bootstrap-typeahead'; 
import './Home.css'

// const hidden = {
//     display: "none"
// }
// const border = {
//     borderBottom: "3px solid #e4e4e4",
//     lineHeight: "16px",
//     padding: ".35rem 0 .55rem"
// }
export class Home extends Component {
    static displayName = Home.name;
    constructor(props) {
        super(props);
        this.state = {
            project: 'gradle-project',
            dependencies: ['Hystrix', 'Actuator', 'MySql', 'Serilog'],
            showMore: false,
            selected_deps: []
        }
         this.handleSelection = this.handleSelection.bind(this);
         this.toggleMore = this.toggleMore.bind(this);
    }
    
    handleSelection(e) {
        console.log("selection " + e);
        //this.state.selected_deps.push(e);
        this.setState(prevState => ({ selected_deps: [...prevState.selected_deps, e ]}))
        this._typeAhead._instance.clear();

        console.log('typeahead' , this._typeAhead);

    }
    toggleMore(e){
        this.setState(prevState => ({showMore: !prevState.showMore}))
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
    let hrefLink = "#";
    return (
        <div>
            <form name="form" action="/starter.zip" method="post"  autoComplete="off">
                <InputSelector title='Project' values={ ["Visual Studio"]}/>
                <InputSelector title='Language' values={["C#", "F#", "VB.NET"]} />
                <InputSelector title='Steeltoe' values={["2.1", "2.2", "3.0"]} />
                                 <div className="line">
                                        <div className="left">Project Metadata</div>
                                        <div className="right">
                                            <div className="project-metadata">

                                                        <div className="control">
                                                        <label>Project Name</label>
                                                        <input className="control-text" name="projectName" placeholder="SteeltoeExample1" tabIndex="1" />
                                                        </div>
                                                        
                                                        {this.state.showMore && 
                                                            <div className="more-block">
                                                            <div id="more-block">
                                                                        <div class="control">
                                                                            <label>Description</label>
                                                                                <input className="control-text" name="description" placeholder="Demo project for Steeltoe" />
                                                                        </div>
                                                                        <RightInputSelector title='.NET Core Version' values={["2.2", "3.0"]} />
            
                                                            </div> 
                                                            </div>
                                                    }
                                                    <div className="control control-submit">
                                                        <p id="more-options">
                                                            <button type="button" onClick={this.toggleMore} className="btn">{this.state.showMore? 'Fewer Options' : 'More options'}</button>
                                                        </p>
                                                        
                                                    </div>
                                                                            </div>
                                                                        </div>
                                                                    </div>
                                                                    <div className="line">
                                                                        <div className="left">
                                                                            <div className="dependencies-label">Dependencies <a href={hrefLink} id="see-all">See all</a></div>
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
                                        ref={(ref) => this._typeAhead = ref}
                                    />
</div>
                                                                                        <div className="no-result" id="noresult-to-add"><em>No result.</em></div>
                                <div className="list" id="list-to-add">
                                  
                                </div>
                            </div>
                          
                                                                                    <div className="col" id="col-dep">
                                                                                        <strong>Selected dependencies</strong>
                                <div className="list light" id="list-added">
                                    {
                                        this.state.selected_deps.map((item) => {
                                            return <div className="item" >
                                                <div className="title">
                                                    {item}
                                                </div>
                                                <div className="description">
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
                                                                        <div className="line row-action">
                                                                            <div className="left">
                                                                                <div className="footer">© 2013-2019 Pivotal Software<br/>start.steeltoe.io is powered by<br/>  
                                                                                <a href="https://dotnetfoundation.org/">.NET foundation</a> and <a href="https://run.pivotal.io/" tabIndex="999">Pivotal Web Services</a></div>
</div>
                                                                                    <div className="right">
                                                                                        <div className="submit">
                                                                                            <button id="btn-generate" type="submit" className="btn btn-primary" tabIndex="4">
                                                                                                <span className="text">Generate Project</span> -
<span className="shortcut"> alt + ⏎</span>
                                                                                            </button>
                                                                                        </div>
                                                                                    </div>
                                                                                </div>
</form>
   </div>

    );
  }
}

        