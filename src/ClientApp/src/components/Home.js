import React, { Component } from 'react'; 
import { InputSelector } from './InputSelector';
import { RightInputSelector } from './RightInputSelector';
import { AutoSuggestSelector } from './AutoSuggestSelector';
import { BottomLinks } from './BottomLinks';
import { InputText } from './InputText';
import { NetProjectSuggestions } from './NetProjectSuggestions';
import './Home.css'


export class Home extends Component {
    static displayName = Home.name;
  
    constructor(props) {
        super(props);
        

        this.state = {
            showMore: false,
            templateType: ".NET Templates",
            level2SelectorType: "net",
            lang: "C#"


        }
            
         this.toggleMore = this.toggleMore.bind(this);
         this.handleInputChange = this.handleInputChange.bind(this);
         
    }
   
    toggleMore(e){
        this.setState(prevState => ({showMore: !prevState.showMore}))
    }
    handleInputChange(name, selectedValue) {
        if (name == "templateType") {
             this.setState({ level2SelectorType: selectedValue == ".NET Templates" ? "net" : "steeltoe"})
        }

        this.setState({ [name]: selectedValue })
        console.log("parent setting hanglechange" , name, selectedValue)
    }
   
  

  render () {
    
    return (
        <div>
            <form name="form" action="/starter.zip" method="post" autoComplete="off">
                    <div>
                    <InputSelector id="steeltoeVersion" title="Steeltoe Version" name="steeltoeVersion" values={["2.1", "2.2", "3.0"]} defaultValue="2.2" onChange={this.handleInputChange} />
                    <AutoSuggestSelector id="deps" available_deps={["CloudFoundry", "Hystrix", "Actuator", "MySql", "Dynamic Logger"]} />
                    </div>

                <div className="line">
                    <div className="left">Project Metadata</div>
                    <div className="right">
                        <div className="project-metadata">

                            <InputText title="Project Name" name="projectName" defaultValue="SteeltoeExample" tabIndex="1" />
                                      <div id="more-block">
                                        <InputText title="Description" name="description" defaultValue="Demo project for Steeltoe" tabIndex="2" />
                                <RightInputSelector title='.NET Core Version' values={["2.2", "3.0"]} />
                            </div>
                           
                         </div>
                    </div>
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

        