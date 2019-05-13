import React, { Component } from 'react'; 
import { InputSelector } from './InputSelector';
import { RightInputSelector } from './RightInputSelector';
import { AutoSuggestSelector } from './AutoSuggestSelector';
import { BottomLinks } from './BottomLinks';
import { InputText } from './InputText';
import './Home.css'


export class Home extends Component {
    static displayName = Home.name;
    constructor(props) {
        super(props);
        this.state = {
            showMore: false
        }
         this.toggleMore = this.toggleMore.bind(this);
         this.handleInputChange = this.handleInputChange.bind(this);
         
    }
   
    toggleMore(e){
        this.setState(prevState => ({showMore: !prevState.showMore}))
    }
    handleInputChange(name, selectedValue){
        console.log('Parent change' + selectedValue)
        this.setState({ [name]: selectedValue})
    }
   
  

  render () {
    let hrefLink = "#";
    return (
        <div>
            <form name="form" action="/starter.zip" method="post" autoComplete="off">
                <InputSelector title='Template' name="templateType" values={ [".NET Templates", "Steeltoe Templates"]} defaultValue="Steeltoe Templates" onChange={this.handleInputChange}/>
                <InputSelector title='Steeltoe' name="steeltoeVersion" values={["2.1", "2.2", "3.0"]} defaultValue="2.2" onChange={this.handleInputChange}/>
                <div className="line">
                    <div className="left">Project Metadata</div>
                    <div className="right">
                        <div className="project-metadata">

                            <InputText title="Project Name" name="projectName" defaultValue="SteeltoeExample" tabIndex="1" />
                            { this.state.showMore && 
                                <div className="more-block">
                                    <div id="more-block">
                                        <InputText title="Description" name="description" defaultValue="Demo project for Steeltoe" tabIndex="2" />
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
                <AutoSuggestSelector id="deps"  />                           
                                                
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

        