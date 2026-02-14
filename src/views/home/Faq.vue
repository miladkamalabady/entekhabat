<template>
  <div>

    <b-alert show variant="success" class="text-center"> در صورتی که سوال شما در لیست زیر نمی‌باشد می توانید <a
        target="_blank" style="color:#145388" href="https://my.medu.ir/app/contact?contactidtag=13"> در این بخش </a>
      ارسال نمایید.
    </b-alert>

    <b-form-input v-model="search" placeholder="جستجو در سوالات..." class="mb-3 text-right" />

    <b-card v-for="(item, index) in filteredFaqs" :key="index" no-body class="mb-2">
      <b-card-header class="p-1" role="tab">
        <b-button block variant="link" class="text-right" v-b-toggle="'faq-' + index">
          {{ item.question }}
        </b-button>
      </b-card-header>

      <b-collapse :id="'faq-' + index" accordion="faq-accordion" role="tabpanel">
        <b-card-body>
          {{ item.answer }}
        </b-card-body>
      </b-collapse>
    </b-card>
  </div>
</template>

<script>
import { isMobile } from "../../utils";
import { mapGetters, mapActions, mapMutations } from "vuex";
import Sidebar from "../../navs/Sidebar.vue";
export default {
  name: "FaqTemplate",
  computed: {
    ...mapGetters([
      "sidebarVisible",
      "processing",
      "loginError",
      "currentUser",
    ]),
    filteredFaqs() {
      if (!this.search) return this.faqs;
      const s = this.search.trim().toLowerCase();
      return this.faqs.filter(faq =>
        faq.question.toLowerCase().includes(s) ||
        faq.answer.toLowerCase().includes(s)
      );
    }
  }, components: {
    Sidebar
  },
  data() {
    return {
      isMobile,
      search: null,
      faqs: [
        {
          "id": 1,
          "question": "در مرحله استقرار رتبه‌بندی (سری اول)، در همۀ آیتم‌ها به‌جز سنوات خدمت تجربی در رتبه بالاتری قرار گرفتم. اکنون مشمول ارتقا هستم. آیا مدارک قبلی کفایت می‌کند؟",
          "answer": "خیر، کلیه امتیازات و مستندات قبلی مربوط به کسب رتبه اولیه (استقرار) بوده و در مرحله ارتقا قابل استفاده نیستند. در این مرحله فقط مدارک و امتیازات کسب‌شده در بازه زمانی «تاریخ آخرین مشمولیت قبلی تا تاریخ مشمولیت جدید» بررسی می‌شود."
        },
      ]

    };
  }
};
</script>
