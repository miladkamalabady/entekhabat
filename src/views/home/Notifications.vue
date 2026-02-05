<template>
  <div>

    <b-card v-for="(item, index) in notifs" :key="index" class="mb-2 notif-card">
      <div class="d-flex justify-content-between align-items-center">
        <div>
          <h6 class="mb-1 font-weight-bold">
            {{ item.title }}
            <b-badge v-if="item.isNew" variant="danger" class="ml-2">
              جدید
            </b-badge>
          </h6>
          <small class="text-muted">{{ item.tarikh }}</small>
        </div>

        <b-button size="sm" variant="outline-primary" @click="openNotif(item)">
          مشاهده
        </b-button>
      </div>
    </b-card>

    <b-modal v-model="showNotif" title="جزئیات اطلاعیه" hide-footer size="lg" @hidden="clearRoute">
      <h5>{{ selectedNotif?.title }}</h5>
      <small class="text-muted">{{ selectedNotif?.tarikh }}</small>
      <hr />
      <div v-html="selectedNotif?.content" class="text-justify"></div>
    </b-modal>


  </div>
</template>

<script>
import { isMobile } from "../../utils";
import { notifs } from "../../data/dataConst";
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
  }, components: {
    Sidebar
  },
  methods: {
    clearRoute() {
      this.$router.replace({ query: {} });
    },
    openNotif(item) {
      this.selectedNotif = item;
      this.showNotif = true;

      // sync URL
      this.$router.replace({
        query: { id: item.id }
      });
    },
    checkRouteForModal() {
      const id = this.$route.query.id;
      if (!id) return;

      const notif = this.notifs.find(n => String(n.id) === String(id));
      if (notif) {
        this.selectedNotif = notif;
        this.showNotif = true;

        // seen شد
        notif.isNew = false;
      }
    }
  }, mounted() {
    this.checkRouteForModal();
  },
  data() {
    return {
      notifs,
      isMobile,
      showNotif: false,
      selectedNotif: null,


    };
  }
};
</script>
<style scoped>
.notif-card {
  transition: 0.2s;
}

.notif-card:hover {
  background: #f9fbff;
}
</style>