import axios from 'axios'

export const defaultMenuType = 'menu-default' // 'menu-default', 'menu-sub-hidden', 'menu-hidden';
export const adminRoot = '/app';
export const searchPath = `${adminRoot}/pages/miscellaneous/search`
export const buyUrl = '#'

export var apiUrlrtb = 'http://192.168.13.63:8080/api';
export var apiUrlhrm = 'https://hrm1.medu.ir/api';

async function doGetRequest() {
  let res = await axios.get('/../config.js');
  apiUrlrtb = (JSON.parse(res.data)[0].apiUrlrtb);
}
doGetRequest();


export const subHiddenBreakpoint = 1440
export const menuHiddenBreakpoint = 768

export const defaultLocale = 'fa'
export const defaultDirection = 'rtl'
export const localeOptions = [
  // { id: 'en', name: 'انگلیسی', direction: 'ltr' },
  // { id: 'ar', name: 'عربی', direction: 'rtl' },
  { id: 'fa', name: 'فارسی', direction: 'rtl' }
]


export const currentUser = {

}

export const isAuthGuardActive = true
export const themeRadiusStorageKey = 'theme_radius'
export const themeSelectedColorStorageKey = 'theme_selected_color'
export const defaultColor = 'light.blueolympic'
export const colors = ['bluenavy', 'blueyale', 'blueolympic', 'greenmoss', 'greenlime', 'purplemonster', 'orangecarrot', 'redruby', 'yellowgranola', 'greysteel']

export const tavana = {
}