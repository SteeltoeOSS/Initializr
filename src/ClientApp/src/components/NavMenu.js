import React, { Component } from 'react';
import './NavMenu.css';
const styles = {
    position: "relative",
    padding: "8px 16px 8px 35px"
};
const logoStyles = {
    margin: "0",
    padding: "8px 0",
    width: "280px",
    font: "2em",
    paddingLeft:"65px",
    textDecoration: "none",
    fontSize:"1rem",
    lineHeight: "1.6rem",
}
const imageStyles = {
    paddingLeft: "20px"
}
const betaStyles = {
    paddingLeft: "40px",
    position: "absolute"
}
export class NavMenu extends Component {
  static displayName = NavMenu.name;
   
  constructor (props) {
        super(props);

        this.toggleNavbar = this.toggleNavbar.bind(this);
        this.state = {
          collapsed: true
        };
    }

  toggleNavbar () {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }

  render () {
      return (

          <header style={styles}>
            <h1 style={logoStyles}>
                <img src="logo-inline.svg" width="180px" height="50px" alt="" style={imageStyles} />
                <img src="beta.png" alt="beta" height="80px" style={betaStyles} />
                <br/>
                Kick start your .NET app
            </h1>
            <ul className="quick-links">
                  <li><a href="https://github.com/steeltoeOSS/initializr" rel="noreferrer noopener" target="_blank" tabIndex="-1">
                      <svg xmlns="http://www.w3.org/2000/svg" aria-label="GitHub" role="img"
                          viewBox="0 0 512 512"><rect
                              width="512" height="512"
                              rx="100%"
                              fill="#1B1817" /><path fill="#fff" d="M335 499c14 0 12 17 12 17H165s-2-17 12-17c13 0 16-6 16-12l-1-50c-71 16-86-28-86-28-12-30-28-37-28-37-24-16 1-16 1-16 26 2 40 26 40 26 22 39 59 28 74 22 2-17 9-28 16-35-57-6-116-28-116-126 0-28 10-51 26-69-3-6-11-32 3-67 0 0 21-7 70 26 42-12 86-12 128 0 49-33 70-26 70-26 14 35 6 61 3 67 16 18 26 41 26 69 0 98-60 120-117 126 10 8 18 24 18 48l-1 70c0 6 3 12 16 12z" />
                      </svg>
                      Github</a></li>
                  
                  <li><a href="https://github.com/SteeltoeOSS/initializr/issues/new?assignees=&labels=feedback%2C+question&template=feedback-or-question.md&title=%5BFeedback%5D" rel="noreferrer noopener" target="_blank" tabIndex="-1">
                      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1000 1000">
                          <g><path d="M826.7,826.7H663.3c-24.5,0-40.8-16.3-40.8-40.8c0-24.5,16.3-40.8,40.8-40.8h163.3c44.9,0,81.7-36.8,81.7-81.7v-490c0-44.9-36.8-81.7-81.7-81.7H173.3c-44.9,0-81.7,36.8-81.7,81.7v490c0,44.9,36.7,81.7,81.7,81.7h163.3c24.5,0,40.8,16.3,40.8,40.8c0,24.5-16.3,40.8-40.8,40.8H173.3C83.5,826.7,10,753.2,10,663.3v-490C10,83.5,83.5,10,173.3,10h653.3C916.5,10,990,83.5,990,173.3v490C990,753.2,916.5,826.7,826.7,826.7L826.7,826.7z" /><path d="M500,990c-12.3,0-20.4-4.1-28.6-12.3L308.1,814.4c-16.3-16.3-16.3-40.8,0-57.2c16.3-16.3,40.8-16.3,57.2,0L500,892l134.8-134.8c16.3-16.3,40.8-16.3,57.2,0c16.3,16.3,16.3,40.8,0,57.2L528.6,977.8C520.4,985.9,512.3,990,500,990L500,990z" /><path d="M745,295.8H255c-24.5,0-40.8-16.3-40.8-40.8s16.3-40.8,40.8-40.8h490c24.5,0,40.8,16.3,40.8,40.8S769.5,295.8,745,295.8L745,295.8z" /><path d="M540.8,459.2H255c-24.5,0-40.8-16.3-40.8-40.8c0-24.5,16.3-40.8,40.8-40.8h285.8c24.5,0,40.8,16.3,40.8,40.8C581.7,442.8,565.3,459.2,540.8,459.2L540.8,459.2z" /><path d="M663.3,622.5H255c-24.5,0-40.8-16.3-40.8-40.8c0-24.5,16.3-40.8,40.8-40.8h408.3c24.5,0,40.8,16.3,40.8,40.8C704.2,606.2,687.8,622.5,663.3,622.5L663.3,622.5z" /></g>
                      </svg>
                      Feedback</a></li>

            </ul>
         </header>
    );
  }
}
