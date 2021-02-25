import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'
import axios from "axios";
import Toaster from '@meforma/vue-toaster';

axios.interceptors.request.use(request => {
    if (store.state.context.jwtToken) {
        request.headers['Authorization'] = 'Bearer ' + store.state.context.jwtToken;
    }
    return request;
  });

createApp(App)
  .use(router)
  .use(store)
  .use(Toaster)
  .mixin({
    created(){
      let opts = { position: "bottom-right" };
      this.$notifyError = (text) => this.$toast.error(text, opts);
      this.$notifySuccess = (text) => this.$toast.success(text, opts);
      this.$notifyNormal = (text) => this.$toast.show(text, opts);
      this.$notifyInfo = (text) => this.$toast.info(text, opts);
    }
  })
  .mount('#app');
