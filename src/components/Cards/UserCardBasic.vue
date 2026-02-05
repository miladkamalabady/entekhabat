<template>
  <div>
  <b-card no-body class="d-flex flex-row p-2 bgshadow">
    <b-card-body class="p-0">
          <div class="d-flex mb-3" style="align-items: center;">
            <ThumbnailImage rounded small :src="data?.img" alt="profile" class-name="muirtl-45do71 m-1" />
            <div><div class="text-right"> {{
              EmployeeInfo?.firstName }} {{
                EmployeeInfo?.lastName }}</div>
                <div class="text-right text-muted"> {{
              EmployeeInfo?.lastPosition }}</div>
                </div>
          </div>
        </b-card-body>
      </b-card>
      <br/>
        <b-card no-body class="d-flex flex-row p-2 bgshadow">
    <b-card-body class="p-0">
          <b-tabs lazy class="w-100">
            <b-tab title="اطلاعات شخصی">
              
              <b-row class="mt-4 m-0">
                <!-- <b-colxx xxs="12" class="text-right mb-2 "><h3>اطلاعات شخصی</h3></b-colxx> -->
                <b-colxx xxs="12" xs="6" class="text-right mb-2"><span class="text-muted">کدملی:</span> <span class="boldlarge">{{
                  EmployeeInfo?.nationalCode }}</span></b-colxx>
                <b-colxx xxs="12" xs="6" class="text-right mb-2"><span class="text-muted">نام پدر:</span> <span class="boldlarge">{{
                  EmployeeInfo?.fatherName }}</span></b-colxx>
              </b-row>
            
              <b-row class="m-0">
                <b-colxx xxs="12" xs="6" class="text-right mb-2"><span class="text-muted"> محل خدمت:</span><span class="boldlarge"> {{
                  EmployeeInfo?.region
                  }}</span></b-colxx>
                <b-colxx xxs="12" xs="6" class="text-right mb-2 "><span class="text-muted">پرسنلی:</span> <span class="boldlarge"> {{
                  EmployeeInfo?.employeeCode }}</span></b-colxx>
                <b-colxx xxs="12" xs="6" class="text-right mb-2"><span class="text-muted">نوع استخدام:</span><span class="boldlarge"> {{
                  EmployeeInfo?.employmentType }}</span></b-colxx>
                <b-colxx xxs="12" xs="6" class="text-right mb-2"><span class="text-muted">آخرین پست سازمانی:</span><span class="boldlarge"> {{
                  EmployeeInfo?.lastPosition }}</span></b-colxx>
                <b-colxx xxs="12" xs="6" class="text-right mb-2"><span class="text-muted">آخرین مدرک تحصیلی:</span><span class="boldlarge"> {{
                  EmployeeInfo?.degree }}</span></b-colxx>
              </b-row>
            </b-tab>
            <b-tab title="تصاویر پرسنلی" class="d-flex" style="align-items: center;">
              <ThumbnailImage rounded small :src="data?.img" alt="profile" class-name="m-1" />
              <div class="text-right mb-2 text-muted mb-2"> {{
                EmployeeInfo?.firstName }} {{
                  EmployeeInfo?.lastName }}</div>
            </b-tab>
          </b-tabs>
    </b-card-body>
  </b-card>
</div>
</template>
<script>
import ThumbnailImage from "./ThumbnailImage";
export default {
  props: ['data', 'employmentStatus', 'EmployeeInfo'],
  components: {
    ThumbnailImage
  }
}
</script>
<style scoped>
.boldlarge{;font-weight:700}

</style>