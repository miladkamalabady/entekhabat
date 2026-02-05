<template>
  <span class="verify-icon" :title="title">
    <!-- ⏳ در حال بررسی -->
    <svg
      v-if="status == 1"
      viewBox="0 0 24 24"
      role="img"
      aria-label="در حال بررسی"
    >
      <path
        fill="#B0B3B8"
        d="M12 0.8 14.3 3.4 17.7 2.6 18.2 6.1 21.6 7.2
           20.4 10.4 23.2 12 20.4 13.6 21.6 16.8
           18.2 17.9 17.7 21.4 14.3 20.6 12 23.2
           9.7 20.6 6.3 21.4 5.8 17.9 2.4 16.8
           3.6 13.6 0.8 12 3.6 10.4 2.4 7.2
           5.8 6.1 6.3 2.6 9.7 3.4 Z"
      />
    </svg>
    <svg
    v-else-if="status==2"
  viewBox="0 0 24 24"
  role="img"
  aria-label="در حال بررسی"
>
      <path
        fill="#B0B3B8"
        d="M12 0.8 14.3 3.4 17.7 2.6 18.2 6.1 21.6 7.2
           20.4 10.4 23.2 12 20.4 13.6 21.6 16.8
           18.2 17.9 17.7 21.4 14.3 20.6 12 23.2
           9.7 20.6 6.3 21.4 5.8 17.9 2.4 16.8
           3.6 13.6 0.8 12 3.6 10.4 2.4 7.2
           5.8 6.1 6.3 2.6 9.7 3.4 Z"
      />
      <g transform="translate(0 0.4)">
        <path
        fill="#fff"
        d="M10.2 14.5 7.5 11.8
           a1 1 0 0 1 1.4-1.4
           l2 2 4.7-4.7
           a1 1 0 1 1 1.4 1.4
           l-5.4 5.4
           a1 1 0 0 1-1.4 0Z"
      />
      </g>
</svg>

    <!-- ✅ تأیید شده -->
    <svg
      v-else-if="status == 3"
      viewBox="0 0 24 24"
      role="img"
      aria-label="تأیید شده"
    >
      <path
        fill="#4CAF50"
        d="M12 0.8 14.3 3.4 17.7 2.6 18.2 6.1 21.6 7.2
           20.4 10.4 23.2 12 20.4 13.6 21.6 16.8
           18.2 17.9 17.7 21.4 14.3 20.6 12 23.2
           9.7 20.6 6.3 21.4 5.8 17.9 2.4 16.8
           3.6 13.6 0.8 12 3.6 10.4 2.4 7.2
           5.8 6.1 6.3 2.6 9.7 3.4 Z"
      />
      <path
        fill="#fff"
        d="M10.2 14.5 7.5 11.8
           a1 1 0 0 1 1.4-1.4
           l2 2 4.7-4.7
           a1 1 0 1 1 1.4 1.4
           l-5.4 5.4
           a1 1 0 0 1-1.4 0Z"
      />
    </svg>

    
    <svg
      v-else-if="!status"
      viewBox="0 0 24 24"
      role="img"
      aria-label="قبل"
    >
      <path
        fill="#3F51B5"
        d="M12 1
           a5 5 0 0 0-5 5
           v3H6
           a2 2 0 0 0-2 2
           v8
           a2 2 0 0 0 2 2
           h12
           a2 2 0 0 0 2-2
           v-8
           a2 2 0 0 0-2-2
           h-1V6
           a5 5 0 0 0-5-5Zm-3 8V6
           a3 3 0 0 1 6 0
           v3Z"
      />
    </svg>
  </span>
</template>

<script>
export default {
  name: "VerifyStatusIcon",
  props: {
    status: {
      required: true
    }
  },
  computed: {
    title() {
      switch (this.status) {
        case 1: return "ثبت اولیه"
        case 2: return "در حال بررسی"
        case 3: return "تأیید شده"
        default: return "قبل"
      }
    }
  }
}
</script>

<style scoped>
.verify-icon svg {
  width: 20px;
  height: 20px;
  vertical-align: middle;
}
</style>
