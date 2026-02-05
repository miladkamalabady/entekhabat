<template>
  <b-form-group class="mb-4 text-right">
    <div
      class="upload-area"
      :class="{ 'is-invalid': !!error, 'disabled': disabled }"
      @dragover.prevent="onDragOver"
      @dragleave.prevent="onDragLeave"
      @drop.prevent="onDrop"
      @click="triggerFileInput"
    >
    
      <div v-if="!preview" class="upload-content text-center">
        <i class="simple-icon-cloud-upload upload-icon"></i>
        <p class="upload-text">فایل‌ها را برای آپلود بکشید و رها کنید<br/>حجم تصویر باید بین 50 تا 200 کیلوبایت باشد<br/>نوع فایل باید jpg باشد</p>
        <b-button
          variant="outline-primary"
          size="sm"
          @click.stop="triggerFileInput"
          class="upload-btn"
        >
          انتخاب فایل
        </b-button>
      </div>

      <img
        v-if="preview"
        :src="preview"
        :alt="title"
        class="preview-image "
      />

      <b-button
        v-if="preview && !disabled"
        variant="outline-danger"
        size="sm"
        @click.stop="removeFile"
        class="remove-btn"
      >
        <i class="simple-icon-trash" ></i>
      </b-button>
      <b-button
        v-if="preview"
        variant="outline-success"
        size="sm"
        @click.stop="showImageModal=true"
        class="eye"
      >
        <i class="simple-icon-eye" ></i>
      </b-button>
    </div>
<b-modal
  v-model="showImageModal"
  size="xl"
  centered
  hide-footer
  title="نمایش سند"
>
  <div class="text-center">
    <img
      v-if="preview"
      :src="preview"
      class="img-fluid rounded"
      style="max-height: 75vh;"
    />
  </div>
</b-modal>
    <!-- <div v-if="error" class="invalid-feedback d-block text-right mt-1">
      {{ error }}
    </div> -->

    <input
      ref="fileInput"
      type="file"
      :accept="accept"
      :disabled="disabled"
      @change="onFileChange"
      style="display: none;"
    />
  </b-form-group>
</template>

<script>
  import { mapGetters, mapMutations, mapActions } from "vuex";
export default {
  name: "CustomImageUploadField",
  props: {
    label: { type: String, default: "تصویر" },
    disabled: { type: Boolean, default:false },
    id: { type: String, required: true },
    value: { type: File, default: null },
    error: { type: String, default: null },
    required: { type: Boolean, default: false },
    accept: { type: String, default: "image/jpeg" },
    title: { type: String, default: "تصویر" },
    disabled: { type: Boolean, default: false }
  },
  emits: ["input", "change"],
  data() {
    return {
      showImageModal:false,
      internalFile: null,
      preview: null,
      isDragging: false
    };
  },computed: {
    ...mapGetters([
      "FilesByIdsInfo",
    ]),
  },
  watch: {
   async FilesByIdsInfo(val){
        const blob = new Blob([val], { type: "image/jpeg" });

         this.internalFile=blob
         this.updatePreview();
    },
    value: {
      immediate: true,
      handler(file) {
        if (file && file !== this.internalFile) {
          
          this.internalFile = file;
          this.updatePreview();
        }
        else
        {
          this.internalFile = null;
          this.preview = null;
        }
      }
    }
  },
  methods: {
    convertToBase64(file) {
      if(file)
      return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = () => resolve(reader.result);
        reader.onerror = (error) => reject(error);
      });
    },
    onFileChange(event) {
      const file = event.target.files[0] || null;
      if(file){
        if (
        file?.name &&
        (file?.size > 200 * 1024 ||
        file?.size < 50 * 1024)
      ) {
        this.$notify(
          "warning",
          "خطا",
          "حجم تصویر گواهی باید بین 50 تا 200 کیلو بایت باشد!",
          {
            duration: 3000,
            permanent: false,
          }
        );
        this.removeFile()
      }else if (
        file?.name &&
        file?.type != "image/jpeg"
      ) {
        this.$notify("warning", "خطا", "نوع تصویر jpg نمی باشد!", {
          duration: 3000,
          permanent: false,
        });
        this.removeFile()
      }
      else{
      this.internalFile = file;
      this.updatePreview();
      this.$emit("input", file);
      this.$emit("change", file);
      }
    }
    },
    updatePreview() {
      if (this.internalFile) {
        
        const reader = new FileReader();
        reader.onload = (e) => {
          this.preview = e.target.result;
        };
        reader.readAsDataURL(this.internalFile);
      } else {
        this.preview = null;
      }
    },
    triggerFileInput() {
      if (this.disabled) return;
      this.$refs.fileInput.click();
    },
    removeFile() {
      this.internalFile = null;
      this.preview = null;
      this.$emit("input", null);
      this.$emit("change", null);
      this.$refs.fileInput.value = '';
    },
    onDragOver() {
      this.isDragging = true;
    },
    onDragLeave() {
      this.isDragging = false;
    },
    onDrop(event) {
      this.isDragging = false;
      const file = event.dataTransfer.files[0];
      
      // if (file && this.accept?.split(',').some(ext => file.type.includes(ext.replace(/\*/g, '')))) {
      if (file) {
        if (
        file?.name &&
        file?.size > 200 * 1024 &&
        file?.size < 50 * 1024
      ) {
        this.$notify(
          "warning",
          "خطا",
          "حجم تصویر گواهی باید بین 50 تا 200 کیلو بایت باشد!",
          {
            duration: 3000,
            permanent: false,
          }
        );
        this.removeFile()
      }else if (
        file?.name &&
        file?.type != "image/jpeg"
      ) {
        this.$notify("warning", "خطا", "نوع تصویر jpg نمی باشد!", {
          duration: 3000,
          permanent: false,
        });
        this.removeFile()
      }else{
        this.internalFile = file;
        this.updatePreview();
        this.$emit("input", file);
        this.$emit("change", file);
      }
      } else {
        this.error = 'فرمت فایل معتبر نیست.';
      }
    }
  }
};
</script>

<style scoped>
.upload-area {
  border: 2px dashed #008ecc;
  border-radius: 8px;
  padding: 2rem;
  text-align: center;
  cursor: pointer;
  background-color: #f8fafc;
  transition: all 0.3s ease;
  position: relative;
  min-height: 150px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-direction: column;
}

.upload-area:hover {
  background-color: #e6f7ff;
  border-color: #006699;
}

.upload-area.is-invalid {
  border-color: #dc3545;
  background-color: #fff5f5;
}

.upload-area.disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.upload-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
}

.upload-icon {
  font-size: 2rem;
  color: #008ecc;
}

.upload-text {
  color: #6c757d;
  margin: 0;
  font-size: 0.9rem;
}

.upload-btn {
  margin-top: 0.5rem;
  padding: 0.5rem 1rem;
  font-size: 0.85rem;
}

.preview-image {
  max-width: 100%;
  max-height: 150px;
  border-radius: 4px;
  object-fit: contain;
  margin-bottom: 0.5rem;
}

.remove-btn {
  margin-top: 0.5rem;
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}

.invalid-feedback {
  width: 100%;
  text-align: right;
  margin-top: 0.25rem;
  font-size: 0.65rem;
  color: #dc3545;
}

.text-right .upload-area {
  direction: rtl;
}

.upload-area.is-dragging {
  background-color: #cce5ff;
  border-color: #006699;
}
</style>