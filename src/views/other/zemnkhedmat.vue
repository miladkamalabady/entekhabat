<template>
  <div class="w-100 px-3" :class="isMobile() ? '' : 'mb-1 mt-1'">
    <b-row style="justify-content: center;">
      <b-colxx v-if="DorehhaActiveForFaragirinfo?.length > 1" xs="3" xxs="12" class=" text-right p-1">
        <b-card style=" border-radius: 20px; ">
        <p class="mb-0 mt-2" style="color: #115ca7">
          ضمن خدمت های فعال شما {{ DorehhaActiveForFaragirinfo?.length }} دوره
        </p>
        <p class="text-center">
          <b-button type="submit" @click="LoginSSO({ form: { siteName: 'ltms' } })" variant="outline-warning"
            :disabled="processing" :class="{
              'btn-multiple-state btn-shadow': true,
              'show-spinner': processing,
              'show-success': !processing && loginError === false,
              'show-fail': !processing && loginError,
            }">
            <span class="value" v-if="DorehhaActiveForFaragirinfo?.length > 0">مشاهده دوره ها </span>
            <span class="value" v-else>ثبت نام </span>
          </b-button>
        </p>
        <p class="mb-0 small" v-if="(DorehhaActiveForFaragirinfo?.length > 0)">این دوره ها با توجه به ابلاغ و منطقه خدمتی
          شما قابل ثبت نام می باشند. </p>
          </b-card>
      </b-colxx>
      <b-colxx  :class="DorehhaActiveForFaragirinfo?.length > 1 ? 'col-xs-9' :''"  xxs="12" class="text-right p-0">
        <div class="" style="border-radius: 20px">
          <b-card-body class="text-center m-auto" style=" border-radius: 20px; ">
            <glide-component :settings="glideIconsOption1" v-if="DorehhaActiveForFaragirinfo?.length > 1">
              <div v-for="(val, index) in DorehhaActiveForFaragirinfo" :key="index" class="text-center px-2"
                :class="isMobile() ? 'px-2' : 'px-3'">
                <b-card class="mt-2 w-100 p-0" style="font-size: small;border-radius: 20px" :style="isMobile() ? 'height: 230px;' : 'height: 150px;'">
                  <b-row>
                    <b-colxx xxs="12" class="text-right">
                      <p class="m-0 cursor-pointer scrollbar" style="
                          color: #115ca7;
                          white-space: nowrap;
                          max-width: 100%;
                          overflow: auto;
                        " @click="LoginSSO({ form: { siteName: 'ltms' } })">
                        دوره {{ val?.Kind }} {{ val.Subj }}
                      </p>
                      <p class="m-0 small">
                        {{ val?.Ostan }}
                        {{ val?.Ostan != val?.Mantaghe ? val?.Mantaghe : "" }}
                      </p>
                    </b-colxx>
                    </b-row>
                     <b-row>
                    <b-colxx lg="6" xxs="12"  class="text-right mb-2" style="color: #757575">
                      <img src="assets/img/pages/timer-02.svg" /> مدت دوره:
                      <span style="color: #141414">{{ val?.Saat }} ساعت </span>
                    </b-colxx>
                    <b-colxx lg="6" xxs="12"  class="text-right mb-2" style="color: #757575">
                      <img src="assets/img/pages/dateicon.svg" /> تاریخ آزمون:
                      <span style="color: #141414">{{ val?.ExamDate }} </span>
                    </b-colxx>
                    </b-row>
                    <b-row>
                    <b-colxx lg="4" xxs="12" v-show="val?.StartTime1" class="text-right mb-2" style="color: #624fb5">
                      <img src="assets/img/pages/clock-01.svg" /> آزمون اول:
                      <span style="color: #141414">{{ val?.StartTime1 }} </span>
                    </b-colxx>
                    <b-colxx lg="4" xxs="12" v-show="val?.StartTime2" class="text-right mb-2" style="color: #624fb5">
                      <img src="assets/img/pages/clock-01.svg" />  آزمون دوم:
                      <span style="color: #141414">{{ val?.StartTime2 }} </span></b-colxx>
                    <b-colxx lg="4" xxs="12" v-show="val?.StartTime3" class="text-right" style="color: #624fb5">
                      <img src="assets/img/pages/clock-01.svg" />  آزمون سوم:
                      <span style="color: #141414">{{ val?.StartTime3 }} </span></b-colxx>
                  </b-row>
                </b-card>
              </div>
            </glide-component>
            <div v-else-if="DorehhaActiveForFaragirinfo?.length == 1" v-for="(val, index) in DorehhaActiveForFaragirinfo" :key="index" class="text-center m-auto">
              <b-card class="mt-2 w-100 p-0" style="  border-radius: 20px">
                <h3 class="mb-0 text-right pr-2 pt-2" style="color: #7231CE">
                  ضمن خدمت های فعال شما
                </h3>
                <p style="font-size: small;" v-if="!isMobile()" class="mb-0 text-right">این دوره ها با توجه به ابلاغ و منطقه خدمتی شما قابل ثبت نام می باشند. </p>
                <b-row style="font-size: small;">
                  <b-colxx xxs="12" class="text-right">
                    <p class="m-0 cursor-pointer scrollbar text-right" style="
                        color: #7231CE;
                        white-space: nowrap;
                        max-width: 100%;
                        overflow: auto;
                      " @click="LoginSSO({ form: { siteName: 'ltms' } })">
                      دوره {{ val?.Kind }} {{ val.Subj }}
                      <span class="text-dark">
                        (
                        {{ val?.Ostan }}
                        {{ val?.Ostan != val?.Mantaghe ? val?.Mantaghe : "" }}
                        )
                      </span>
                    </p>
                  </b-colxx>
                  <b-colxx xl="12" v-if="val?.Saat" class=" mb-2  d-flex" :style="isMobile() ? 'flex-direction: column;text-align:center' : ''"
                    style="color: #757575;justify-content: space-between;">
                    <span class="pr-2 ">
                      <img src="assets/img/pages/timer-02.svg" /> مدت دوره:
                      <span style="color: #141414">{{ val?.Saat }} ساعت </span>
                    </span>
                    <span v-if="val?.ExamDate" class="text-right mb-2" style="color: #757575">
                      <img src="assets/img/pages/dateicon.svg" /> تاریخ آزمون:
                      <span style="color: #141414">{{ val?.ExamDate }} </span>
                    </span>
                    <span v-if="val?.StartTime1" class="text-right mb-2" style="color: #624fb5">
                      <img src="assets/img/pages/clock-01.svg" /> زمان آزمون اول:
                      <span style="color: #141414">{{ val?.StartTime1 }} </span>
                    </span>
                    <span xxs="12" v-if="val?.StartTime2" class="text-right mb-2" style="color: #624fb5">
                      <img src="assets/img/pages/clock-01.svg" /> زمان آزمون دوم:
                      <span style="color: #141414">{{ val?.StartTime2 }} </span></span>
                    <span v-if="val?.StartTime3" class="text-right pl-2" style="color: #624fb5">
                      <img src="assets/img/pages/clock-01.svg" /> زمان آزمون سوم:
                      <span style="color: #141414">{{ val?.StartTime3 }}  </span>
                    </span>
                  </b-colxx>
                  <b-colxx xxs="12" class="text-right">
                    <b-button type="submit" @click="LoginSSO({ form: { siteName: 'ltms' } })" variant="outline-warning"
                      :disabled="processing" :class="{
                        'btn-multiple-state btn-shadow': true,
                        'show-spinner': processing,
                        'show-success': !processing && loginError === false,
                        'show-fail': !processing && loginError,
                      }">
                      <span class="value" v-if="DorehhaActiveForFaragirinfo?.length > 0">مشاهده دوره ها </span>
                      <span class="value" v-else>ثبت نام </span>
                    </b-button>
                  </b-colxx>
                </b-row>
              </b-card>
            </div>
            <div v-else-if="isMobile() && type==4" style='display: flex;align-items: center;   font-size: small;'>
              <p style="position: absolute;padding-right: 10px;right: 0;left: 0;">
                <span >ضمن خدمت فعالی برای شما یافت نشد!</span><br/>
                <b-button variant="warning" :style="isMobile() ? 'left:0' : ''" style="background-color: #F6AD2B;" size="sm">مشاهده دوره ها</b-button>
                <br/><span style="color:#9A9A9A; font-size: 0.7rem;">این دوره ها با توجه به ابلاغ و حکم شما قابل دسترس می باشد.</span>
              </p>
              <img class="cursor-pointer" style="width: 100%;" :style="isMobile() ? 'height:120px' : 'height:100px'" @click="LoginSSO({ form: { siteName: 'ltms' } })"
                src="assets/img/zemnkhedmatB.png" />
            </div>
            <div v-else-if="type==4">
              <b-alert show>دوره‌ای برای ثبت نام یافت نشد!</b-alert>
            </div>
          </b-card-body>
        </div>
      </b-colxx>
    </b-row>
  </div>
</template>
<script>
import GlideComponent from "../../components/Carousel/GlideComponent";

import { mapGetters, mapMutations, mapActions } from "vuex";
import { isMobile } from "../../utils";
export default {
  props:['type'],
  components: {
    GlideComponent,
  },
  data() {
    return {
      isMobile,
      glideIconsOption1: {
        peek: { before: -200, after: -200 },
        gap: 5,
        perView: 3,
        startAt: 1,
        type: "carousel",
        // autoplay: 5000,
        animationDuration: 2000,
        breakpoints: { 450: { perView: 3 } },
        hideNav: true,
        swipeThreshold: true,
        shownavcircle: true,
      },
    };
  },
  computed: {
    ...mapGetters([
      "currentUser",
      "loginError",
      "processing",
      "DorehhaActiveForFaragirinfo",
    ]),
  },
  mounted() {
    
    if (!this.DorehhaActiveForFaragirinfo)
      this.DorehhaActiveForFaragir();

  },
  methods: {
    ...mapMutations([]),
    ...mapActions([
      "LoginSSO",
      "DorehhaActiveForFaragir",
    ]),

  },
  watch: {
  },
};
</script>
<style scoped>
.card-body {
  padding: 5px;
}
</style>
