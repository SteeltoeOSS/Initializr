import React, { Component } from 'react'; 
import Autosuggest from 'react-autosuggest';

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }
const languages = [
    {
        name: 'C',
        year: 1972
    },

    {
        name: 'C#',
        year: 2000
    },

    {
        name: 'C++',
        year: 1983
    },

    {
        name: 'Clojure',
        year: 2007
    },

    {
        name: 'Elm',
        year: 2012
    },

    {
        name: 'Go',
        year: 2009
    },

    {
        name: 'Haskell',
        year: 1990
    },

    {
        name: 'Java',
        year: 1995
    },

    {
        name: 'Javascript',
        year: 1995
    },

    {
        name: 'Perl',
        year: 1987
    },

    {
        name: 'PHP',
        year: 1995
    },

    {
        name: 'Python',
        year: 1991
    },

    {
        name: 'Ruby',
        year: 1995
    },

    {
        name: 'Scala',
        year: 2003
    }];



// https://developer.mozilla.org/en/docs/Web/JavaScript/Guide/Regular_Expressions#Using_Special_Characters
function escapeRegexCharacters(str) {
    return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

function getSuggestions(value) {
    const escapedValue = escapeRegexCharacters(value.trim());

    if (escapedValue === '') {
        return [];
    }

    const regex = new RegExp('^' + escapedValue, 'i');

    return languages.filter(language => regex.test(language.name));
}

function getSuggestionValue(suggestion) {
    return suggestion.name;
}

function renderSuggestion(suggestion) {
    return (
        React.createElement("span", null, suggestion.name));

}

export class Suggestions extends Component {
    constructor() {
        super();
        _defineProperty(this, "onChange",



            (event, { newValue, method }) => {
                this.setState({
                    value: newValue
                });

            }); _defineProperty(this, "onSuggestionsFetchRequested",

                ({ value }) => {
                    this.setState({
                        suggestions: getSuggestions(value)
                    });

                }); _defineProperty(this, "onSuggestionsClearRequested",

                    () => {
                        this.setState({
                            suggestions: []
                        });

                    }); _defineProperty(this, "onSuggestionSelected",

                        () => {
                            this.setState({
                                value: ''
                            });

                        }); this.state = { value: '', suggestions: [] };
    }

    render() {
        const { value, suggestions } = this.state;
        const inputProps = {
            placeholder: "Type 'c'",
            value,
            onChange: this.onChange
        };


        return (
            <Autosuggest
                suggestions={suggestions}
                onSuggestionsFetchRequested={this.onSuggestionsFetchRequested}
                onSuggestionsClearRequested={this.onSuggestionsClearRequested}
                getSuggestionValue={getSuggestionValue}
                renderSuggestion={renderSuggestion}
                inputProps={inputProps}
            />);

    }
}
