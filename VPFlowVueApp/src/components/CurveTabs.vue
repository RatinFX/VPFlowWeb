<script setup lang="ts">
import Tabs from "./ui/tabs/Tabs.vue";
import TabsContent from "./ui/tabs/TabsContent.vue";
import TabsList from "./ui/tabs/TabsList.vue";
import TabsTrigger from "./ui/tabs/TabsTrigger.vue";
</script>

<template>
  <Tabs default-value="library" class="flex flex-col h-full">
    <TabsList class="grid w-full grid-cols-2">
      <TabsTrigger value="library">Library</TabsTrigger>
      <TabsTrigger value="custom">Custom</TabsTrigger>
    </TabsList>
    <div class="overflow-auto border-red-400 border-2">
      <TabsContent value="library">
        <div class="flex flex-wrap gap-2">
          <div v-for="_ in 20" class="w-20 h-20 border-cyan-500 border-2">
            lib
          </div>
        </div>
      </TabsContent>
      <TabsContent value="custom">
        <div class="flex flex-wrap gap-2">
          <div v-for="_ in 10" class="w-20 h-20 border-cyan-500 border-2">
            cust
          </div>
        </div>
      </TabsContent>
    </div>
  </Tabs>
</template>
