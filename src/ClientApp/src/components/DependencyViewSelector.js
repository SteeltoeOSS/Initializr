import React, { Component } from 'react';
import ReactGA from 'react-ga';

export class DependencyViewSelector extends Component {
    static displayName = DependencyViewSelector.name;

  constructor (props) {
    super(props);
      this.state = {
          quickSearch: 'active',
          list: ''
      };
      this.onClick = this.onClick.bind(this);
  }

    onClick(e) {
        ReactGA.event({
            category: 'View Selector',
            action: this.state.quickSearch? 'quicksearch': 'list' 
        });
     
      this.setState({
          quickSearch: e === 'quicksearch' ? 'active' : '',
          list: e === 'list' ? 'active' : ''
      });
    
      this.props.onChange(e);
  }

  render () {
    return (
        <div className="tab">
            <div className="tab-container">
                <a href="#" className={'quick-search ' + this.state.quickSearch} aria-label="Search" onClick={() => { this.onClick('quicksearch') }}  >
                    <svg aria-hidden="true" focusable="false" data-icon="search" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" className="icon-search">
                        <path fill="currentColor" d="M505 442.7L405.3 343c-4.5-4.5-10.6-7-17-7H372c27.6-35.3 44-79.7 44-128C416 93.1 322.9 0 208 0S0 93.1 0 208s93.1 208 208 208c48.3 0 92.7-16.4 128-44v16.3c0 6.4 2.5 12.5 7 17l99.7 99.7c9.4 9.4 24.6 9.4 33.9 0l28.3-28.3c9.4-9.4 9.4-24.6.1-34zM208 336c-70.7 0-128-57.2-128-128 0-70.7 57.2-128 128-128 70.7 0 128 57.2 128 128 0 70.7-57.2 128-128 128z"></path></svg>
                </a>
                <a href="#" onClick={() => { this.onClick('list') }} aria-label="List" className={'list ' + this.state.list}  >
                    <svg aria-hidden="true" focusable="false" data-icon="list" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" className="icon-list">
                        <path fill="currentColor" d="M128 116V76c0-8.837 7.163-16 16-16h352c8.837 0 16 7.163 16 16v40c0 8.837-7.163 16-16 16H144c-8.837 0-16-7.163-16-16zm16 176h352c8.837 0 16-7.163 16-16v-40c0-8.837-7.163-16-16-16H144c-8.837 0-16 7.163-16 16v40c0 8.837 7.163 16 16 16zm0 160h352c8.837 0 16-7.163 16-16v-40c0-8.837-7.163-16-16-16H144c-8.837 0-16 7.163-16 16v40c0 8.837 7.163 16 16 16zM16 144h64c8.837 0 16-7.163 16-16V64c0-8.837-7.163-16-16-16H16C7.163 48 0 55.163 0 64v64c0 8.837 7.163 16 16 16zm0 160h64c8.837 0 16-7.163 16-16v-64c0-8.837-7.163-16-16-16H16c-8.837 0-16 7.163-16 16v64c0 8.837 7.163 16 16 16zm0 160h64c8.837 0 16-7.163 16-16v-64c0-8.837-7.163-16-16-16H16c-8.837 0-16 7.163-16 16v64c0 8.837 7.163 16 16 16z">
                        </path></svg></a>
            </div>

        </div>
    );
  }
}
