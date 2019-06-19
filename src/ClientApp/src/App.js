import ReactGA from 'react-ga'
import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';

ReactGA.initialize('UA-114912118-2');
ReactGA.pageview(window.location.pathname + window.location.search);
export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
      </Layout>
    );
  }
}
