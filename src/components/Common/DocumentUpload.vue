<template>
  <div class="document-upload mb-3">
    <label class="form-label">{{ title }}</label>

    <!-- کادر Drag & Drop -->
    <div
      class="drop-zone"
      :class="{ 'drag-over': isDragOver }"
      @dragover.prevent="isDragOver = true"
      @dragleave.prevent="isDragOver = false"
      @drop.prevent="handleDrop"
      @click="triggerFileInput"
    >
      <p v-if="!preview && !localFile">فایل را اینجا بکشید یا کلیک کنید</p>
      <img v-if="preview" :src="preview" class="thumbnail" />
      <span v-else-if="localFile">{{ localFile.name }}</span>
    </div>

    <!-- input مخفی برای انتخاب فایل -->
    <input
      type="file"
      ref="fileInput"
      class="d-none"
      @change="handleFileChange"
      accept="image/*,.pdf"
    />
  </div>
</template>

<script>
export default {
  name: "DocumentUpload",
  props: {
    file: Object,
    title: String
  },
  data() {
    return {
      isDragOver: false,
      localFile: null,
      preview: null
    };
  },
  watch: {
    file: {
      immediate: true,
      handler(val) {
        if (val && val.preview) this.preview = val.preview;
        if (val && val.raw) this.localFile = val.raw;
      }
    }
  },
  methods: {
    triggerFileInput() {
      this.$refs.fileInput.click();
    },
    handleFileChange(event) {
      const files = event.target.files;
      if (!files || !files.length) return;
      this.localFile = files[0];
      this.createPreview(this.localFile);
    },
    handleDrop(event) {
      this.isDragOver = false;
      const files = event.dataTransfer.files;
      if (!files || !files.length) return;
      this.localFile = files[0];
      this.createPreview(this.localFile);
    },
    createPreview(file) {
      if (!file) return;

      if (file.type.startsWith("image/")) {
        const reader = new FileReader();
        reader.onload = e => {
          this.preview = e.target.result;
          this.$emit("update:file", { name: file.name, preview: this.preview, raw: file });
        };
        reader.readAsDataURL(file);
      } else {
        this.preview = null;
        this.$emit("update:file", { name: file.name, preview: null, raw: file });
      }
    }
  }
};
</script>

<style scoped>
.document-upload {
  margin-bottom: 15px;
}
.drop-zone {
  height: 120px;
  border: 2px dashed #ccc;
  border-radius: 6px;
  display: flex;
  justify-content: center;
  align-items: center;
  text-align: center;
  transition: background-color 0.2s, border-color 0.2s;
  cursor: pointer;
  padding: 10px;
  font-size: 14px;
  color: #666;
}
.drop-zone.drag-over {
  border-color: #3f51b5;
  background-color: #f0f4ff;
}

.thumbnail {
  max-width: 100px;
  max-height: 100px;
  border-radius: 4px;
  border: 1px solid #ccc;
  object-fit: cover;
}
</style>
