import React, { Component } from 'react';
//import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';
const styles = {
    position: "relative",
    padding: "8px 16px 8px 24px"
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
                      <img src="logo-inline.svg" width="180px" height="50px" />
                      <br/>
                  Kick start your application
</h1>   
         </header>
    );
  }
}
