<template>
  <div>
    <div class="icon-cards-row" v-if="Slidersinfo?.length > 0">
      <glide-component v-if="Slidersinfo?.length > 0" :settings="glideIconsOption">
        <div
          v-for="(f, index) in Slidersinfo?.filter(x => ((x.title === null) || (x.title == 2 && isMobile()) || (x.title == 1 && !isMobile())))"
          :key="`slide_${index}`" class="headerDashboard" style="background-color:transparent">
          <b-row>
            <b-colxx xxs="12" class="">
              <div class="boxText p-0">
                <a v-if="f.href" :href="f.href">
                  <img :src="f.imageUrl" style="width:100%;max-width:100%;max-height:400px;border-radius:10px" />
                </a>
                <img v-else :src="f.imageUrl" style="width:100%;max-width:100%;max-height:400px;border-radius:10px" />
              </div>
            </b-colxx>
          </b-row>
        </div>
      </glide-component>

      <div v-if="Slidersinfo?.length == 0" class="headerDashboardpeople mt-3">
        <b-row>
          <b-colxx xl="8" lg="8" md="8" xs="12" xxs="12">
            <div class="boxText">
              <p>شهروند گرامی سلام !</p>
              <h2>مفتخــریم که درخدمت شما هستیم ...</h2>
            </div>
          </b-colxx>
        </b-row>
      </div>
    </div>
    <div v-else-if="currentUser?.result?.type == 3" class="mt-2">
      <glide-component :settings="glideIconsOption">
        <!-- <div class="headerDashboardpeople p-0" style="background-color: #d6d1f3;">
          <div @click="LoginSSO({ form: { siteName: 'ltms' } })">
            <img src=" assets/img/zemnkhedmatA.webp" :style="isMobile() ? 'max-width: 100%;' : 'max-width: 100%; '"
              style="border-radius: 10px;" />
          </div>
        </div> -->
        <div class="banner p-2 ">
          <router-link :to="{ name: 'contactTajrobe' }">
            <img src="assets/img/tajrobe.webp" alt="لپ‌تاپ">
          </router-link>
        </div>
        <div class="banner p-1">
          <router-link :to="{ name: 'contactBime' }">
            <img src="assets/img/bimeh.webp" alt="لپ‌تاپ">
          </router-link>
        </div>

      </glide-component>
    </div>


    <div v-else class=" mt-3">
      <b-row>
        <b-colxx xxs="12" class="">
          <div class="boxText cursor-pointer p-0">
            <img @click="showModal('tamas')" src="assets/img/nahal1.png"
              style="width:80%;max-width:100%;max-height:400px;border-radius:10px" />
          </div>
        </b-colxx>
      </b-row>
    </div>
    <b-modal id="tamas" hide-footer ref="tamas" size="md" title="تماس ">
      <div class="text-center">
        جهت درخواست با شماره 1540 تماس بگیرید
      </div>
    </b-modal>
  </div>
</template>
<script>
import GlideComponent from "../../components/Carousel/GlideComponent";
import { mapActions, mapGetters, mapMutations } from "vuex";
import { isMobile } from '../../utils'
export default {
  components: {
    "glide-component": GlideComponent,
  },
  mounted() {
        if (false && !this.Slidersinfo) {
      this.setSlidersinfo(null)
      this.GetSliders({ "viewType": 2 })
    }
  }
  , computed: {
    ...mapGetters(["Slidersinfo", "currentUser"])
  }, methods: {
    ...mapMutations(["setSlidersinfo"]),
    ...mapActions(["GetSliders", "LoginSSO"]),
    showModal(val) {
      this.$refs[val].show();
    }
  },
  data() {
    return {
      isMobile,
      glideIconsOption: {
        gap: 10,
        perView: 3,
        peek: { before: 0, after: 40 },
        // autoplay: 4000,
        animationDuration: 1000,
        type: "carousel",
        breakpoints: {
          320: {
            perView: 1
          },
          576: {
            perView: 1
          },
          1600: {
            perView: 2
          },
          1800: {
            perView: 2
          }
        },
        // hideNav: true
      }
    };
  }
};
</script>
<style scoped>
.typewriter1 {
  display: inline-block;
  overflow: hidden;
  white-space: nowrap;
  animation: typing 5 steps(40, end) infinite, blink 0.75s step-end infinite;
  animation-delay: 5s, 5s;
}

.typewriter {
  color: #010490;
  display: inline-block;
  overflow: hidden;
  white-space: nowrap;
  animation: typing 5s steps(40, end) infinite, blink 0.75s step-end infinite;
}

@keyframes typing {
  from {
    width: 0
  }

  to {
    width: 100%
  }
}

@keyframes blink {
  50% {
    border-color: transparent
  }
}

.gradient-button {
  background: linear-gradient(270deg, #00b894, #00cec9, #00b894);
  background-size: 400% 400%;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  animation: gradientMove 6s ease infinite;
  transition: transform 0.2s;
  box-shadow: 0 4px 12px rgba(0, 206, 201, 0.4);
}

.gradient-button:hover {
  transform: scale(1.05);
}

@keyframes gradientMove {
  0% {
    background-position: 0% 50%;
  }

  50% {
    background-position: 100% 50%;
  }

  100% {
    background-position: 0% 50%;
  }
}

.animated-gradient-text {
  background: linear-gradient(270deg, #f50707, #ffffff, #4a554e, #06b6d4);
  background-size: 600% 600%;
  font-size: large;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  animation: shineText 8s ease infinite;
}

@keyframes shineText {
  0% {
    background-position: 0% 50%;
  }

  50% {
    background-position: 100% 50%;
  }

  100% {
    background-position: 0% 50%;
  }
}


.banner img {
  border-radius: 10px;
  width: 100%;
  max-width: 100%;
}
</style>