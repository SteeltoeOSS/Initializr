import './Home.css'

import React, { Component } from 'react'; 
import { InputSelector } from './InputSelector';
import { RightInputSelector } from './RightInputSelector';
import { DependenciesSelector } from './DependenciesSelector';
import { BottomLinks } from './BottomLinks';
import { InputText } from './InputText';
import ReactGA from 'react-ga';

export class Home extends Component {
    static displayName = Home.name;
    static initialState  =  {
        showMore: false,
        templateType: ".NET Templates",
        level2SelectorType: "net",
        lang: "C#",
        steeltoeVersion: "2.4.0",
        steeltoeVersionInvalid: "",
        targetFrameworkVersion: "netcoreapp2.2"
    };
    constructor(props) {
        super(props);
        

        this.state = Home.initialState;
        this.toggleMore = this.toggleMore.bind(this);
        this.handleInputChange = this.handleInputChange.bind(this);
        this.OnSubmit = this.OnSubmit.bind(this);
         
    }
    OnSubmit(e){
        this.setState(Home.initialState);
        this.trackSubmitEvent(e);
    }
    trackSubmitEvent(e) {
        let i;
        const elements = e.target.elements;

        ReactGA.event({
            category: 'Generated Project',
            action: 'Clicked',
            label: ''
        });
        ReactGA.event({
            category: 'Generated Project',
            action: 'Steeltoe-Framework',
            label: elements["steeltoeVersion"].value
        });
        ReactGA.event({
            category: 'Generated Project',
            action: 'Net-Framework',
            label: elements["targetFrameworkVersion"].value
        });
        //Send events for dependencies
        const deps = elements["dependencies"].value;
        const depArray = deps.split(',');
        for(i = 0; i < depArray.length ; i++)
        {
            ReactGA.event({
                category: 'Generated Project',
                action: 'Dependency',
                label: depArray[i]
            });
        }
    }
    toggleMore(e){
        this.setState(prevState => ({showMore: !prevState.showMore}))
    }
    handleInputChange(name, selectedValue) {

        //if (name === "templateType") {
        //     this.setState({ level2SelectorType: selectedValue === ".NET Templates" ? "net" : "steeltoe"})
        //}
        
        if (name === "targetFrameworkVersion" && selectedValue === "netcoreapp3.1" && this.state.steeltoeVersion === "2.3.0") {
            this.setState({
                "steeltoeVersion": "2.4.0",
                [name]: selectedValue,
                "steeltoeVersionInvalid": ""
            })
        }
        else if(name === "steeltoeVersion" && selectedValue === "2.3.0" && this.state.targetFrameworkVersion === "netcoreapp3.1"){
            this.setState({
                "steeltoeVersion": "2.4.0",
                "steeltoeVersionInvalid": "2.4.0 is the lowest version compatible with netcoreapp3.1",
            })
        }
        else {
            this.setState({ 
                [name]: selectedValue,
                "steeltoeVersionInvalid": ""
            });
        }
       // console.log("parent setting hanglechange" , name, selectedValue)
    }
   
  

  render () {
    
    return (
        <div>
            <form name="form" action="/starter.zip" method="post" autoComplete="off" onSubmit={this.OnSubmit} >
                <div>
                    <InputSelector id="steeltoeVersion" title="Steeltoe Version" name="steeltoeVersion" values={["2.3.0", "2.4.0"]} defaultValue="2.4.0" selectedValue={this.state.steeltoeVersion} onChange={this.handleInputChange} invalidText={this.state.steeltoeVersionInvalid} />
                    <div className="line">
                        <div className="left">Project Metadata</div>
                        <div className="right">
                            <div className="project-metadata">

                                <InputText title="Project Name" name="projectName" defaultValue="MyCompany.SteeltoeExample" tabIndex="1" required pattern="^(?:((?!\d)\w+(?:\.(?!\d)\w+)*)\.)?((?!\d)\w+)$" onInput={(e) => e.target.setCustomValidity("")} onInvalid={(e) => e.target.setCustomValidity("ProjectName must be a valid C# Identifier: ex. MyCompany.MyProject")} />
                                <div id="more-block">
                                    <InputText title="Description" name="description" defaultValue="Demo project for Steeltoe" tabIndex="2" />
                                    <RightInputSelector title='Target Framework' name="targetFrameworkVersion" values={["netcoreapp2.1", "netcoreapp2.2", "netcoreapp3.1"]} defaultValue="netcoreapp2.2" selectedValue={this.state.targetFrameworkVersion}  onChange={this.handleInputChange} />
                                </div>
                           
                            </div>
                        </div>
                    </div>      
                    <DependenciesSelector id="deps" />
                    <br/>
                </div>          
                <div className="line row-action">
                    <BottomLinks />
                    <div className="right">
                        <div className="submit">
                            <button id="btn-generate" type="submit" className="btn btn-primary" tabIndex="4">
                                <span className="text">Generate Project</span> - <span className="shortcut"> alt + ⏎</span>
                            </button>
                        </div>
                    </div>
                </div>
            </form>
        </div>

    );
  }
}

        