import React, { Component } from 'react'; 
import { InputSelector } from './InputSelector';
import { RightInputSelector } from './RightInputSelector';
import { AutoSuggestSelector } from './AutoSuggestSelector';
//import { Suggestions } from './Suggestions';
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
            showMore: false,
        }
         this.toggleMore = this.toggleMore.bind(this);
    }
   
    toggleMore(e){
        this.setState(prevState => ({showMore: !prevState.showMore}))
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
                                   <AutoSuggestSelector />                           
                                                                   
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

        