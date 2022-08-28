import fs from 'fs';
import path from 'path';
import matter from 'gray-matter';
import html from 'remark-html';
import { remark } from 'remark';

// Based on the Next.js "blog-starter" example.

const postsDirectory = path.join(process.cwd(), 'posts');

export async function getPost(slug) {
  const fullPath = path.join(postsDirectory, `${slug}.md`);
  const fileContents = fs.readFileSync(fullPath, 'utf8');
  const matterResult = matter(fileContents);
  const content = await remark().use(html).process(matterResult.content);

  return { slug, content: content.toString(), ...matterResult.data };
}

export async function getAllPostSlugs() {
  const filenames = await fs.promises.readdir(postsDirectory);
  return filenames
    .filter(filename => filename.endsWith('.md'))
    .map(filename => filename.substring(0, filename.length - 3));
}

export async function getAllPosts() {
  const slugs = await getAllPostSlugs();
  const posts = await Promise.all(slugs.map(getPost));
  return posts
    .sort((lhs, rhs) => {
      const lhsTimestamp = new Date(lhs.publishedTimestamp);
      const rhsTimestamp = new Date(rhs.publishedTimestamp);
      if (lhsTimestamp != rhsTimestamp) {
        return lhsTimestamp < rhsTimestamp ? 1 : -1;
      } else {
        return 0;
      }
    });
}
