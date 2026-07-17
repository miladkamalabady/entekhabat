import {
  apiUrlrtb
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


   if (user?.token) config.headers.Authorization = `Bearer ${user.token}`;

  if (data.name.includes('GetFileById') || data.name.includes('HrmImage')) {

    config.responseType= 'arraybuffer'
    // config.responseType= 'blob'
  }
  if (commit && commit.inline_insert) config.url = urlInlineInsertResolver(data.params, apiUrlrtb + url);

  config[config.method == "POST" || config.method == "PUT" ? "data" : "params"] = data.params;


config.headers.tenant = 'root'
config.headers['Accept-Language'] = 'fa-IR'


  await axios(config)
    .then(response => {
      
      retr = response?.data
       if (retr.data.status && retr.data.status == 500) {
        commit.commit('setError', retr.data?.Title)
      } 
      if (commit && commit.process == undefined) {
        setTimeout(() => {
          commit.commit('setProcessing', false)
        }, 100)
      }
    })
    .catch(error => {
      if (error.response?.data?.exception) {
        commit.commit('setError', error.response?.data?.exception)
      }
      else {
        let err = "پاسخی از سرور دریافت نشد! مجدد اقدام نمایید"
        commit.commit('setError', err)
      }
      if(error.message==('Network Error') && location.href!='http://localhost:2000/unauthorized'){
        let err = "خطای ارتباط با سرور!";
        commit.commit('setError', err)
      }
      else if (error.response?.status == 401) {
        localStorage.removeItem('user');
        localStorage.clear();
        commit.commit('setLogout');
        setTimeout(() => {
          location.replace("/");
        }, 1500)
      }
      setTimeout(() => {
        commit.commit('clearError')
      }, 3000)
      retr = false

    });
  return retr
}
