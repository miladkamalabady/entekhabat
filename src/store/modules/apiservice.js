import {
  apiUrlrtb, apiUrlhrm
} from '../../constants/config'


import userinfo from '../../store/modules/user'
import global from '../../store/modules/serviceApi'
import axios from 'axios'
import qs from 'qs';



export default async function (data, commit) {
  const urlInlineInsertResolver = (params, url) => {
    return url.replace(/\{\{(.*?)\}\}/ig, (target) => {
      return params[target.replace(/[\{\}]/ig, '')]
    });
  };
commit.commit('setProcessing', true)
  let retr = "";
  let { url, method } = global[data.name];

  let user = userinfo.state.currentUser;

  let config = {
    url: apiUrlrtb + url,
    method: method || 'POST',
    retry: 1,
    retryDelay: 32000,
    timeout: 32000,
    headers: {
      
    },
    paramsSerializer: (params) => {
      return qs.stringify(params, { arrayFormat: 'repeat' });
    },
  };


  if (data.name == 'logoutsso')
    config.url = "https://sso.my.gov.ir/logout"


  if (url.includes('/rtb/')) {
    const newUrl = url.replace('/rtb/', '/')
    config.url = `${apiUrlrtb}${newUrl}`
 
  } else if (url.includes('/RtbN/')) {
    const newUrl = url.replace('/RtbN/', '/')
    config.url = `https://profile1.medu.ir/rtb/api/rtb${newUrl}`
    // if (location.origin.includes('http://localhost'))
    // config.url = `http://192.168.13.63:8012/api/rtb${newUrl}`

  } else if (url.includes('/External/')) {
    const newUrl = url
    config.url = `${apiUrlhrm}${newUrl}`
  } 

   if (user?.token) config.headers.Authorization = `Bearer ${user.token}`;

  if (data.name.includes('GetFileById') || data.name.includes('HrmImage')) {

    config.responseType= 'arraybuffer'
    // config.responseType= 'blob'
  }
  if (commit && commit.inline_insert) config.url = urlInlineInsertResolver(data.params, apiUrlrtb + url);


  config[config.method == "POST" || config.method == "PUT" ? "data" : "params"] = data.params;


  if (config?.data?.captchaResponse) {
    config.headers.captchaResponse = config?.data?.captchaResponse;
    config.headers.CaptchaHash = config?.data?.CaptchaHash;
  }

  if (config?.params?.captchaResponse) {
    config.headers.captchaResponse = config?.params?.captchaResponse;
    config.headers.CaptchaHash = config?.params?.CaptchaHash;
  }

  await axios(config)
    .then(response => {
      retr = response.data
      if ((response?.status == 200 && response?.data=='')|| (response?.status == 204)) {
        commit.commit('clearError')
        retr=true
      } else if ((response?.status == 200) || retr?.status == 'OK' || retr?.ResponseCode == 1) {
        
      }
      else if (retr.data.status && retr.data.status == 500) {
        commit.commit('setError', retr.data?.Title)
      } 
      if (commit && commit.process == undefined) {
        setTimeout(() => {
          commit.commit('setProcessing', false)
        }, 100)
      }
    })
    .catch(error => {
      if (!error.response?.status) {
        commit.commit('clearError')

        return { status: 200, data: true };
      }
      else if (error.response?.status == 401) {
        localStorage.removeItem('user');
        localStorage.clear();

        let err = "زمان ورود شما به پایان رسیده است، لطفا مجدد وارد شوید";
        commit.commit('setError', err)
        setTimeout(() => {
          // if (location.href == 'https://profile.medu.ir/')
            location.replace("https://my.medu.ir");
        }, 500)

      } else if (error.response?.status == 500) {
        commit.commit('setError', error.response.data.Title)
      }
      else if (error.response?.data) {
        const data = error.response.data;
        const arrayFailed = Object.entries(data).map((arr) => ({
          message: arr,
        }));
        
        
        let err = ""
        arrayFailed.map(function (value, key) {
          err = err + value.message + "<br/>"
        });
        
        commit.commit('setError', err)

      } else if (error?.response?.status == 404) {
        let err = "خطای دریافت اطلاعات! مجدد تلاش نمایید"
        commit.commit('setError', err)
      }
      else {
        let err = "پاسخی از سرور دریافت نشد! مجدد اقدام نمایید"
        commit.commit('setError', err)
      }
      setTimeout(() => {
        commit.commit('clearError')
      }, 3000)
      retr = false

    });
  return retr
}
