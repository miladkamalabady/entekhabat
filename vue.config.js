const CopyPlugin = require('copy-webpack-plugin')
var webpack = require('webpack');
const fs = require('fs')
const packageJson = fs.readFileSync('./package.json')
const version = JSON.parse(packageJson).version || 0

module.exports = {

  pages: {
    index: {
      entry: './src/index.js',
      template: 'public/index.html',
      filename: 'index.html',
      chunks: ['chunk-vendors', 'chunk-common', 'index']
    }
  },
  devServer: {
    clientLogLevel: 'warning',
    hot: true,
    disableHostCheck: true,
    contentBase: 'dist',
    compress: true,
    open: true,
    overlay: { warnings: false, errors: true },
    publicPath: '/',
    port: 2000, 
    quiet: true,
    watchOptions: {
      poll: false,
      ignored: /node_modules/
    }
  },
  css: {
    extract: {
      filename: 'assets/css/[name].css?_hash=[hash]',
      chunkFilename: 'assets/css/[name].css?_hash=[hash]'
    }
  },
  chainWebpack: config => {
    config.plugins.delete('prefetch-index'),
      config
        .output
        .filename('assets/js/[name].js?_hash=[hash]')
        .chunkFilename('assets/js/[name].js?_hash=[hash]')
    config.module
      .rule('vue')
      .use('vue-loader')
      .tap(args => {
        args.compilerOptions.whitespace = 'preserve'
      })
  },
  
  productionSourceMap: false,
  assetsDir: './assets/',
  // publicPath: '/',
  outputDir: 'dist',
  filenameHashing: false,
  configureWebpack: {
    plugins: [
      new CopyPlugin({
        patterns: [
          { from: './src/assets/img', to: 'assets/img' },
          { from: './src/assets/logos', to: 'assets/logos' },
          { from: './src/assets/fonts', to: 'assets/fonts' }
        ],
      }),
      // new webpack.IgnorePlugin({
        // resourceRegExp: /icons|calendar/,
        // contextRegExp: /bootstrap-vue/,
      //   checkResource (resource, context) {
      //     if (context.includes('bootstrap-vue')) {
      //       console.log(resource, ':::', context)
      //       // check console to figure out how the resource is used
      //       // update the function until it's satisfies your case
      //       // then move to regexp if you wish
      //     }
      //     return false
      //   },
      // }),
      new webpack.DefinePlugin({
        'process.env': {
          PACKAGE_VERSION: '"' + version + '"'
        }
      }),
    ]
  }
}