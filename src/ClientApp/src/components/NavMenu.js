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
                  <img src="beta.png" height="80px" style={betaStyles} />
               <br/>
              Kick start your .NET app
              </h1>
         </header>
    );
  }
}
