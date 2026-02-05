<template>
  <img
    :alt="alt"
    :src="src"
    :class="`img-thumbnail list-thumbnail align-self-center ${className}
    ${rounded!=undefined && 'rounded-circle'} ${small!=undefined && 'small'}`"
  />
</template>
<script>
    export default {
        props: ["alt", "src", "className", "rounded", "small"]
    };
</script>
<style scoped>
.muirtl-45do71 {
    width:100px;height:100px;
    text-align: center;
    /* object-fit: cover; */
    color: transparent;
    text-indent: 10000px;
}

</style>
