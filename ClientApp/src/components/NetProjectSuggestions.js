import React, { Component } from 'react'; 
import Autosuggest from 'react-autosuggest';

const netTable = [
    ["Console Application", "console", "C#, F#, VB", "Common / Console"],
    ["Class library", "classlib", "C#, F#, VB", "Common / Library"],
    ["Unit Test Project", "mstest", "C#, F#, VB", "Test / MSTest"],
    ["NUnit 3 Test Project", "nunit", "C#, F#, VB", "Test / NUnit"],
    ["NUnit 3 Test Item", "nunit - test", "C#, F#, VB", "Test / NUnit"],
    ["xUnit Test Project", "xunit", "C#, F#, VB", "Test / xUnit"],
    ["Razor Page", "page", "C#", "Web / ASP.NET"],
    ["MVC ViewImports", "viewimports", "C#", "Web / ASP.NET"],
    ["MVC ViewStart", "viewstart", "C#", "Web / ASP.NET"],
    ["ASP.NET Core Empty", "web", "C#, F#", "Web / Empty"],
    ["ASP.NET Core Web App(Model - View - Controller)", "mvc", "C#, F#", "Web / MVC"],
    ["ASP.NET Core Web App", "webapp", "C#", "Web / MVC / Razor Pages"],
    ["ASP.NET Core with Angular", "angular", "C#", "Web / MVC / SPA"],
    ["ASP.NET Core with React.js", "react", "C#", "Web / MVC / SPA"],
    ["ASP.NET Core with React.js and Redux", "reactredux", "C#", "Web / MVC / SPA"],
    ["Razor Class Library", "razorclasslib", "C#", "Web / Razor / Library / RazorClassLibrary"],
    ["ASP.NET Core Web API", "webapi", "C#, F#", "Web / WebAPI"]
];

let netElements = netTable.map(row => ({ name: row[0], short:  row[1], lang: row[2], tags : row[3] }));

// https://developer.mozilla.org/en/docs/Web/JavaScript/Guide/Regular_Expressions#Using_Special_Characters
function escapeRegexCharacters(str) {
    return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

function getSuggestions(value, lang) {
    const escapedValue = escapeRegexCharacters(value.trim());

    if (escapedValue === '') {
        return [];
    }
    const regex = new RegExp(escapedValue, 'i');
    var regexFilter = (item) => item != null && item.lang.includes(lang) && (regex.test(item.name) || regex.test(item.short));
    return netElements.filter(regexFilter);
}

function getSuggestionValue(suggestion) {
    return suggestion.name;
}

function renderSuggestion(suggestion) {
    return [
        <div className="title">
            {suggestion.name}
        </div>,
        <div key={suggestion.name} className="desc">
           Tags:{suggestion.tags}
            </div>
    ];

}

export class NetProjectSuggestions extends Component {
    constructor(props) {
        super();

        this.state = {
            value: '',
            suggestions: []
        };
    }

    onChange = (event, { newValue, method }) => {
        this.setState({
            value: newValue
        });
    };

    onSuggestionsFetchRequested = ({ value }) => {
        this.setState({ suggestions: getSuggestions(value, this.props.lang)});
    };

    onSuggestionsClearRequested = () => {
        this.setState({
            suggestions: []
        });
    };

    render() {
        const { value, suggestions } = this.state;
        const inputProps = {
            placeholder: "Web, Api etc",
            value,
            onChange: this.onChange,
            className: "control-inner-text"
        };

        return (
            <div className="line">
                <div className="left">
                    Project Type
                </div>
                <div className="right">
<div className="project-metadata">
                <div className="control control-text">
                    <Autosuggest
                        suggestions={suggestions}
                        onSuggestionsFetchRequested={this.onSuggestionsFetchRequested}
                        onSuggestionsClearRequested={this.onSuggestionsClearRequested}
                        getSuggestionValue={getSuggestionValue}
                        renderSuggestion={renderSuggestion}
                        inputProps={inputProps}
                            />
                    </div>
                </div>
                </div>
            </div>
                );

    }
}
