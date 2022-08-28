import Layout from '../../../components/layout';
import Head from 'next/head';
import commonStyles from '../../../styles/common.module.css';
import { getAllPostSlugs, getPost } from '../../../lib/posts';

export default function Post({ post }) {
  return (
    <Layout>
      <Head>
        <title>{`${post.title} | Fluent Feeds`}</title>
      </Head>

      <section className={commonStyles.section}>
        <h1>{post.title}</h1>
        <small>posted by {post.author} on {post.timestamp}</small>
        <div dangerouslySetInnerHTML={{ __html: post.content }} />
      </section>
    </Layout>
  );
}

export async function getStaticPaths() {
  const slugs = await getAllPostSlugs();
  const paths = slugs
    .map(slug => {
      return { params: { slug } };
    });
  return { paths, fallback: false };
}

export async function getStaticProps({ params }) {
  const post = await getPost(params.slug);
  return { props: { post } }
}
