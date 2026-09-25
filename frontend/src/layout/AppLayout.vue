<template>
  <div class="min-h-screen bg-ink-50">
    <Sidebar :open="sidebarOpen" @close="sidebarOpen = false" />

    <div class="lg:pl-[248px]">
      <Topbar @toggle-menu="sidebarOpen = !sidebarOpen" />

      <main class="mx-auto w-full max-w-[1500px] px-4 py-6 sm:px-6 lg:px-8 lg:py-8">
        <router-view v-slot="{ Component }">
          <transition name="page" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import Sidebar from './Sidebar.vue'
import Topbar from './Topbar.vue'

const sidebarOpen = ref(false)
</script>

<style scoped>
.page-enter-active,
.page-leave-active {
  transition: opacity 0.15s ease, transform 0.15s ease;
}
.page-enter-from {
  opacity: 0;
  transform: translateY(6px);
}
.page-leave-to {
  opacity: 0;
}
</style>
