import Vue from 'vue'
import App from './App'


// BootstrapVue add
import { BootstrapVue, IconsPlugin } from 'bootstrap-vue'
Vue.use(IconsPlugin)
// Router & Store add
import router from './router'
import store from './store'
// Multi Language Add
import en from './locales/en.json'
import fa from './locales/fa.json'
import VueI18n from 'vue-i18n'
// Notification Component Add
import Notifications from './components/Common/Notification'
// Breadcrumb Component Add
import Breadcrumb from './components/Common/Breadcrumb'
// RefreshButton Component Add
import RefreshButton from './components/Common/RefreshButton'
// Colxx Component Add
import Colxx from './components/Common/Colxx'
// Perfect Scrollbar Add
import vuePerfectScrollbar from 'vue-perfect-scrollbar'
import contentmenu from 'v-contextmenu'
import VueLineClamp from 'vue-line-clamp'
// import VueScrollTo from 'vue-scrollto'
// Vue.use(VueScrollTo);
// import watermark from '@serializedowen/vue-img-watermark'
// Vue.use(watermark)
// import VueAudio from 'vue-audio-better'
// Vue.use(VueAudio)

import $moment from 'vue-jalali-moment';  

Vue.use($moment);

import { getCurrentLanguage } from './utils'

/////////////
import VueCoreVideoPlayer from 'vue-core-video-player'

Vue.use(VueCoreVideoPlayer)
/////////////

/////////////
Vue.use(BootstrapVue);
Vue.use(VueI18n);
const messages = {en: en,fa: fa };
const locale = getCurrentLanguage();
const i18n = new VueI18n({
  locale: locale,
  fallbackLocale: 'fa',
  messages
});

// import moment from 'vue-jalali-moment'
// Vue.use(moment)


Vue.use(Notifications);
// Vue.use(require('vue-shortkey'));
Vue.use(contentmenu);


Vue.use(VueLineClamp, {
  importCss: false
});

Vue.component('piaf-breadcrumb', Breadcrumb);
Vue.component('b-refresh-button', RefreshButton);
Vue.component('b-colxx', Colxx);
Vue.component('vue-perfect-scrollbar', vuePerfectScrollbar);


export default new Vue({
  i18n,
  router,
  store,
  render: h => h(App)
}).$mount('#app')
Vue.config.productionTip = false